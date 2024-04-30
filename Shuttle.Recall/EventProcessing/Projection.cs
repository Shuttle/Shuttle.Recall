using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Reflection;

namespace Shuttle.Recall
{
    public class Projection
    {
        private static readonly Type HandlerContextType = typeof(EventHandlerContext<>);
        private static readonly Type EventHandlerType = typeof(IEventHandler<>);
        private static readonly Type AsyncEventHandlerType = typeof(IAsyncEventHandler<>);
        private readonly Dictionary<Type, object> _asyncEventHandlers = new Dictionary<Type, object>();
        private readonly Dictionary<Type, ContextConstructorInvoker> _constructorCache = new Dictionary<Type, ContextConstructorInvoker>();
        private readonly Dictionary<Type, object> _eventHandlers = new Dictionary<Type, object>();
        private readonly EventStoreOptions _eventStoreOptions;

        private readonly Dictionary<Type, ContextMethodInvoker> _methodCache = new Dictionary<Type, ContextMethodInvoker>();
        private readonly Dictionary<Type, AsyncContextMethodInvoker> _methodCacheAsync = new Dictionary<Type, AsyncContextMethodInvoker>();

        private Guid? _projectionsQueueId;

        public Projection(EventStoreOptions eventStoreOptions, string name, long sequenceNumber)
        {
            _eventStoreOptions = Guard.AgainstNull(eventStoreOptions, nameof(eventStoreOptions));

            Name = Guard.AgainstNullOrEmptyString(name, nameof(name));
            SequenceNumber = sequenceNumber;
            AggregationId = Guid.Empty;
        }

        public Guid AggregationId { get; private set; }

        public IEnumerable<Type> EventTypes => _eventHandlers.Keys;

        public string Name { get; }
        public long SequenceNumber { get; private set; }

        public async Task<Projection> AddEventHandlerAsync(object handler)
        {
            Guard.AgainstNull(handler, nameof(handler));

            if (!_eventStoreOptions.Asynchronous)
            {
                throw new InvalidOperationException(string.Format(Resources.ProjectionAddEventHandlerSynchronousException, handler.GetType().FullName));
            }

            var typesAddedCount = 0;

            foreach (var interfaceType in handler.GetType().InterfacesAssignableTo(AsyncEventHandlerType))
            {
                var type = interfaceType.GetGenericArguments()[0];

                if (_asyncEventHandlers.ContainsKey(type))
                {
                    throw new InvalidOperationException(string.Format(Resources.DuplicateAsyncEventHandlerEventTypeException, handler.GetType().FullName, type.FullName));
                }

                _asyncEventHandlers.Add(type, handler);

                typesAddedCount++;
            }

            if (typesAddedCount == 0)
            {
                throw new EventProcessingException(string.Format(Resources.InvalidAsyncEventHandlerTypeExpection, handler.GetType().FullName));
            }

            return await Task.FromResult(this);
        }

        public Projection AddEventHandler(object handler)
        {
            Guard.AgainstNull(handler, nameof(handler));

            if (_eventStoreOptions.Asynchronous)
            {
                throw new InvalidOperationException(string.Format(Resources.ProjectionAddEventHandlerAsynchronousException, handler.GetType().FullName));
            }

            var typesAddedCount = 0;

            foreach (var interfaceType in handler.GetType().InterfacesAssignableTo(EventHandlerType))
            {
                var type = interfaceType.GetGenericArguments()[0];

                if (_eventHandlers.ContainsKey(type))
                {
                    throw new InvalidOperationException(string.Format(Resources.DuplicateEventHandlerEventTypeException, handler.GetType().FullName, type.FullName));
                }

                _eventHandlers.Add(type, handler);

                typesAddedCount++;
            }

            if (typesAddedCount == 0)
            {
                throw new EventProcessingException(string.Format(Resources.InvalidEventHandlerTypeExpection, handler.GetType().FullName));
            }

            return this;
        }

        public Projection Aggregate(Guid aggregationId)
        {
            if (!AggregationId.Equals(Guid.Empty))
            {
                throw new InvalidOperationException(string.Format(Resources.ProjectionAggregationAlreadyAssignedException, Name));
            }

            AggregationId = aggregationId;

            return this;
        }

        public void Assigned(Guid projectionsQueueId)
        {
            _projectionsQueueId = projectionsQueueId;
        }

        public void Process(EventEnvelope eventEnvelope, object domainEvent, PrimitiveEvent primitiveEvent, CancellationToken cancellationToken)
        {
            ProcessAsync(eventEnvelope, domainEvent, primitiveEvent, cancellationToken, true).GetAwaiter().GetResult();
        }

        public async Task ProcessAsync(EventEnvelope eventEnvelope, object domainEvent, PrimitiveEvent primitiveEvent, CancellationToken cancellationToken)
        {
            await ProcessAsync(eventEnvelope, domainEvent, primitiveEvent, cancellationToken, false).ConfigureAwait(false);
        }

        private async Task ProcessAsync(EventEnvelope eventEnvelope, object domainEvent, PrimitiveEvent primitiveEvent, CancellationToken cancellationToken, bool sync)
        {
            Guard.AgainstNull(eventEnvelope, nameof(eventEnvelope));
            Guard.AgainstNull(domainEvent, nameof(domainEvent));
            Guard.AgainstNull(primitiveEvent, nameof(primitiveEvent));

            if (primitiveEvent.SequenceNumber <= SequenceNumber)
            {
                return;
            }

            var domainEventType = Type.GetType(eventEnvelope.AssemblyQualifiedName, true);

            try
            {
                if (!(sync ? _eventHandlers : _asyncEventHandlers).TryGetValue(domainEventType, out var eventHandler))
                {
                    return;
                }

                ContextMethodInvoker contextMethod = null;
                AsyncContextMethodInvoker asyncContextMethod = null;

                if (!_constructorCache.TryGetValue(domainEventType, out var contextConstructor))
                {
                    contextConstructor = new ContextConstructorInvoker(domainEventType);

                    _constructorCache.Add(domainEventType, contextConstructor);
                }

                if (sync && !_methodCache.TryGetValue(domainEventType, out contextMethod))
                {
                    var interfaceType = EventHandlerType.MakeGenericType(domainEventType);
                    var method = eventHandler.GetType().GetInterfaceMap(interfaceType).TargetMethods.SingleOrDefault();

                    if (method == null)
                    {
                        throw new ProcessEventMethodMissingException(string.Format(Resources.ProcessEventMethodMissingException, _eventHandlers[domainEventType].GetType().FullName, domainEventType.FullName));
                    }

                    contextMethod = new ContextMethodInvoker(domainEventType, eventHandler.GetType().GetInterfaceMap(EventHandlerType.MakeGenericType(domainEventType)).TargetMethods.SingleOrDefault());

                    _methodCache.Add(domainEventType, contextMethod);
                }

                if (!sync && !_methodCacheAsync.TryGetValue(domainEventType, out asyncContextMethod))
                {
                    var interfaceType = AsyncEventHandlerType.MakeGenericType(domainEventType);
                    var method = eventHandler.GetType().GetInterfaceMap(interfaceType).TargetMethods.SingleOrDefault();

                    if (method == null)
                    {
                        throw new ProcessEventMethodMissingException(string.Format(Resources.ProcessEventMethodMissingException, _eventHandlers[domainEventType].GetType().FullName, domainEventType.FullName));
                    }

                    asyncContextMethod = new AsyncContextMethodInvoker(domainEventType, eventHandler.GetType().GetInterfaceMap(AsyncEventHandlerType.MakeGenericType(domainEventType)).TargetMethods.SingleOrDefault());

                    _methodCacheAsync.Add(domainEventType, asyncContextMethod);
                }

                var handlerContext = contextConstructor.CreateHandlerContext(eventEnvelope, domainEvent, primitiveEvent, cancellationToken);

                if (sync)
                {
                    contextMethod.Invoke(eventHandler, handlerContext);
                }
                else
                {
                    await asyncContextMethod.InvokeAsync(eventHandler, handlerContext).ConfigureAwait(false);
                }
            }
            finally
            {
                SequenceNumber = primitiveEvent.SequenceNumber;
            }
        }

        public void Release(Guid projectionsQueueId)
        {
            if (_projectionsQueueId.HasValue && _projectionsQueueId.Equals(projectionsQueueId))
            {
                _projectionsQueueId = null;

                return;
            }

            throw new InvalidOperationException(Resources.ExceptionInvalidProjectionRelease);
        }

        internal class AsyncContextMethodInvoker
        {
            private readonly InvokeHandler _invoker;

            public AsyncContextMethodInvoker(Type messageType, MethodInfo methodInfo)
            {
                var dynamicMethod = new DynamicMethod(string.Empty,
                    typeof(Task), new[] { typeof(object), typeof(object) },
                    HandlerContextType.Module);

                var il = dynamicMethod.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);

                il.EmitCall(OpCodes.Callvirt, methodInfo, null);
                il.Emit(OpCodes.Ret);

                _invoker = (InvokeHandler)dynamicMethod.CreateDelegate(typeof(InvokeHandler));
            }

            public async Task InvokeAsync(object handler, object handlerContext)
            {
                await _invoker.Invoke(handler, handlerContext).ConfigureAwait(false);
            }

            private delegate Task InvokeHandler(object handler, object handlerContext);
        }

        internal class ContextConstructorInvoker
        {
            private readonly ConstructorInvokeHandler _constructorInvoker;

            public ContextConstructorInvoker(Type messageType)
            {
                var dynamicMethod = new DynamicMethod(string.Empty, typeof(object),
                    new[]
                    {
                        typeof(EventEnvelope),
                        typeof(object),
                        typeof(PrimitiveEvent),
                        typeof(CancellationToken)
                    }, HandlerContextType.Module);

                var il = dynamicMethod.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldarg_2);
                il.Emit(OpCodes.Ldarg_3);

                var contextType = HandlerContextType.MakeGenericType(messageType);
                var constructorInfo = contextType.GetConstructor(new[]
                {
                    typeof(EventEnvelope),
                    messageType,
                    typeof(PrimitiveEvent),
                    typeof(CancellationToken)
                });

                il.Emit(OpCodes.Newobj, constructorInfo);
                il.Emit(OpCodes.Ret);

                _constructorInvoker = (ConstructorInvokeHandler)dynamicMethod.CreateDelegate(typeof(ConstructorInvokeHandler));
            }

            public object CreateHandlerContext(EventEnvelope eventEnvelope, object @event, PrimitiveEvent primitiveEvent, CancellationToken cancellationToken)
            {
                return _constructorInvoker(eventEnvelope, @event, primitiveEvent, cancellationToken);
            }

            private delegate object ConstructorInvokeHandler(EventEnvelope eventEnvelope, object message, PrimitiveEvent primitiveEvent, CancellationToken cancellationToken);
        }

        internal class ContextMethodInvoker
        {
            private readonly InvokeHandler _invoker;

            public ContextMethodInvoker(Type messageType, MethodInfo methodInfo)
            {
                var dynamicMethod = new DynamicMethod(string.Empty,
                    typeof(void), new[] { typeof(object), typeof(object) },
                    HandlerContextType.Module);

                var il = dynamicMethod.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);

                il.EmitCall(OpCodes.Callvirt, methodInfo, null);
                il.Emit(OpCodes.Ret);

                _invoker = (InvokeHandler)dynamicMethod.CreateDelegate(typeof(InvokeHandler));
            }

            public void Invoke(object handler, object handlerContext)
            {
                _invoker.Invoke(handler, handlerContext);
            }

            private delegate void InvokeHandler(object handler, object handlerContext);
        }
    }
}