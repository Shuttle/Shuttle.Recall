using System;
using System.Collections.Generic;
using System.Threading;
using Shuttle.Core.Contract;
using Shuttle.Core.Logging;
using Shuttle.Core.Reflection;

namespace Shuttle.Recall
{
    public class Projection
    {
        private static readonly Type EventHandlerType = typeof(IEventHandler<>);
        private readonly Dictionary<Type, object> _eventHandlers = new Dictionary<Type, object>();

        private readonly ILog _log;
        private Guid? _projectionsQueueId;

        public Projection(string name, long sequenceNumber)
        {
            Guard.AgainstNullOrEmptyString(name, nameof(name));

            Name = name;
            SequenceNumber = sequenceNumber;
            AggregationId = Guid.Empty;

            _log = Log.For(this);
        }

        public string Name { get; }
        public long SequenceNumber { get; private set; }
        public Guid AggregationId { get; private set; }

        public IEnumerable<Type> EventTypes => _eventHandlers.Keys;

        public bool HandlesType(Type type)
        {
            Guard.AgainstNull(type, nameof(type));

            return _eventHandlers.ContainsKey(type);
        }

        public Projection AddEventHandler(object handler)
        {
            Guard.AgainstNull(handler, nameof(handler));

            var typesAddedCount = 0;

            foreach (var interfaceType in handler.GetType().InterfacesAssignableTo(EventHandlerType))
            {
                var type = interfaceType.GetGenericArguments()[0];

                if (_eventHandlers.ContainsKey(type))
                {
                    throw new InvalidOperationException(string.Format(Resources.DuplicateHandlerEventTypeException,
                        handler.GetType().FullName, type.FullName));
                }

                _eventHandlers.Add(type, handler);

                typesAddedCount++;
            }

            if (typesAddedCount == 0)
            {
                throw new EventProcessingException(string.Format(Resources.InvalidEventHandlerType,
                    handler.GetType().FullName));
            }

            return this;
        }

        public void Process(EventEnvelope eventEnvelope, object domainEvent, PrimitiveEvent primitiveEvent,
            CancellationToken cancellationToken)
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
                if (!HandlesType(domainEventType))
                {
                    if (Log.IsTraceEnabled)
                    {
                        _log.Trace(string.Format(Resources.TraceTypeNotHandled, Name,
                            eventEnvelope.AssemblyQualifiedName));
                    }

                    return;
                }

                var contextType = typeof(EventHandlerContext<>).MakeGenericType(domainEventType);
                var method = _eventHandlers[domainEventType].GetType().GetMethod("ProcessEvent", new[] {contextType});

                if (method == null)
                {
                    throw new ProcessEventMethodMissingException(string.Format(
                        Resources.ProcessEventMethodMissingException,
                        _eventHandlers[domainEventType].GetType().FullName,
                        domainEventType.FullName));
                }

                var handlerContext =
                    Activator.CreateInstance(contextType, eventEnvelope, domainEvent, primitiveEvent, cancellationToken);

                method.Invoke(_eventHandlers[domainEventType], new[] {handlerContext});
            }
            finally
            {
                SequenceNumber = primitiveEvent.SequenceNumber;
            }
        }

        public Projection Aggregate(Guid aggregationId)
        {
            if (!AggregationId.Equals(Guid.Empty))
            {
                throw new InvalidOperationException(
                    string.Format(Resources.ProjectionAggregationAlreadyAssignedException, Name));
            }

            AggregationId = aggregationId;

            return this;
        }

        public void Assigned(Guid projectionsQueueId)
        {
            _projectionsQueueId = projectionsQueueId;
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

        public void Skip(long sequenceNumber)
        {
            SequenceNumber = sequenceNumber;
        }
    }
}