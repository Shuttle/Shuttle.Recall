using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Reflection;

namespace Shuttle.Recall;

public class Projection
{
    private static readonly Type HandlerContextType = typeof(EventHandlerContext<>);
    private static readonly Type AsyncEventHandlerType = typeof(IEventHandler<>);
    private readonly Dictionary<Type, object> _asyncEventHandlers = new();
    private readonly Dictionary<Type, ContextConstructorInvoker> _constructorCache = new();
    private readonly Dictionary<Type, object> _eventHandlers = new();

    private readonly Dictionary<Type, AsyncContextMethodInvoker> _methodCacheAsync = new();

    private Guid? _projectionsQueueId;

    public Projection(string name, long sequenceNumber)
    {

        Name = Guard.AgainstNullOrEmptyString(name);
        SequenceNumber = sequenceNumber;
        AggregationId = Guid.Empty;
    }

    public Guid AggregationId { get; private set; }

    public IEnumerable<Type> EventTypes => _eventHandlers.Keys;

    public string Name { get; }
    public long SequenceNumber { get; private set; }

    public async Task<Projection> AddEventHandlerAsync(object handler)
    {
        Guard.AgainstNull(handler);

        var typesAddedCount = 0;

        foreach (var interfaceType in handler.GetType().InterfacesCastableTo(AsyncEventHandlerType))
        {
            var type = interfaceType.GetGenericArguments()[0];

            if (!_asyncEventHandlers.TryAdd(type, handler))
            {
                throw new InvalidOperationException(string.Format(Resources.DuplicateAsyncEventHandlerEventTypeException, handler.GetType().FullName, type.FullName));
            }

            typesAddedCount++;
        }

        if (typesAddedCount == 0)
        {
            throw new EventProcessingException(string.Format(Resources.InvalidAsyncEventHandlerTypeExpection, handler.GetType().FullName));
        }

        return await Task.FromResult(this);
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

    public async Task ProcessAsync(EventEnvelope eventEnvelope, object domainEvent, PrimitiveEvent primitiveEvent, CancellationToken cancellationToken)
    {
        Guard.AgainstNull(eventEnvelope);
        Guard.AgainstNull(domainEvent);
        Guard.AgainstNull(primitiveEvent);

        if (primitiveEvent.SequenceNumber <= SequenceNumber)
        {
            return;
        }

        var domainEventType = Guard.AgainstNull(Type.GetType(eventEnvelope.AssemblyQualifiedName, true));

        try
        {
            if (!_asyncEventHandlers.TryGetValue(domainEventType, out var eventHandler))
            {
                return;
            }

            if (!_constructorCache.TryGetValue(domainEventType, out var contextConstructor))
            {
                contextConstructor = new(domainEventType);

                _constructorCache.Add(domainEventType, contextConstructor);
            }

            if (!_methodCacheAsync.TryGetValue(domainEventType, out var asyncContextMethod))
            {
                var interfaceType = AsyncEventHandlerType.MakeGenericType(domainEventType);
                var methodInfo = eventHandler.GetType().GetInterfaceMap(interfaceType).TargetMethods.SingleOrDefault();

                if (methodInfo == null)
                {
                    throw new ProcessEventMethodMissingException(string.Format(Resources.ProcessEventMethodMissingException, _eventHandlers[domainEventType].GetType().FullName, domainEventType.FullName));
                }

                //var methodInfo = Guard.AgainstNull(eventHandler.GetType().GetInterfaceMap(AsyncEventHandlerType.MakeGenericType(domainEventType)).TargetMethods.SingleOrDefault());
                
                asyncContextMethod = new(methodInfo);

                _methodCacheAsync.Add(domainEventType, asyncContextMethod);
            }

            var handlerContext = contextConstructor.CreateHandlerContext(eventEnvelope, domainEvent, primitiveEvent, cancellationToken);

            await asyncContextMethod.InvokeAsync(eventHandler, handlerContext).ConfigureAwait(false);
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

        public AsyncContextMethodInvoker(MethodInfo methodInfo)
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

            il.Emit(OpCodes.Newobj, Guard.AgainstNull(constructorInfo));
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