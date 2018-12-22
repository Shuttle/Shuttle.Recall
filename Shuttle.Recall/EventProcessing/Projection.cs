using System;
using System.Collections.Generic;
using Shuttle.Core.Contract;
using Shuttle.Core.Reflection;
using Shuttle.Core.Threading;

namespace Shuttle.Recall
{
    public class Projection
    {
        private static readonly Type EventHandlerType = typeof(IEventHandler<>);
        private readonly Dictionary<Type, object> _eventHandlers = new Dictionary<Type, object>();

        public Projection(string name, long sequenceNumber, string machineName, string baseDirectory)
        {
            Guard.AgainstNullOrEmptyString(machineName, nameof(machineName));
            Guard.AgainstNullOrEmptyString(baseDirectory, nameof(baseDirectory));
            Guard.AgainstNullOrEmptyString(name, nameof(name));

            MachineName = machineName;
            BaseDirectory = baseDirectory;
            Name = name;
            SequenceNumber = sequenceNumber;
        }

        public string MachineName { get; }
        public string BaseDirectory { get; }
        public string Name { get; }
        public long SequenceNumber { get; private set; }

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
            IThreadState threadState)
        {
            Guard.AgainstNull(eventEnvelope, nameof(eventEnvelope));
            Guard.AgainstNull(domainEvent, nameof(domainEvent));
            Guard.AgainstNull(primitiveEvent, nameof(primitiveEvent));
            Guard.AgainstNull(threadState, nameof(threadState));

            var domainEventType = Type.GetType(eventEnvelope.AssemblyQualifiedName, true);

            if (!HandlesType(domainEventType))
            {
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
                Activator.CreateInstance(contextType, eventEnvelope, domainEvent, primitiveEvent, threadState);

            method.Invoke(_eventHandlers[domainEventType], new[] {handlerContext});

            SequenceNumber = primitiveEvent.SequenceNumber;
        }
    }
}