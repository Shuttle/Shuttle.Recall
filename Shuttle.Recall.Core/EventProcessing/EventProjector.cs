using System;
using System.Collections.Generic;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Core
{
    public class EventProjector : IEventProjector
    {
        private static readonly Type EventHandlerType = typeof(IEventHandler<>);
        private readonly Dictionary<Type, object> _eventHandlers = new Dictionary<Type, object>();

        public EventProjector(string name)
        {
            Guard.AgainstNullOrEmptyString(name, "name");

            Name = name;
        }

        public string Name { get; private set; }

        public bool HandlesType(Type type)
        {
            Guard.AgainstNull(type, "type");

            return _eventHandlers.ContainsKey(type);
        }

        public EventProjector AddEventHandler(object handler)
        {
            Guard.AgainstNull(handler, "handler");

            if (!EventHandlerType.IsInstanceOfType(handler))
            {
                throw new EventProcessingException(string.Format(RecallResources.InvalidEventHandlerType, handler.GetType().FullName));
            }

            var type = handler.GetType().GetGenericArguments()[0];

            _eventHandlers.Add(type, handler);

            return this;
        }

        public void Process(object domainEvent)
        {
            Guard.AgainstNull(domainEvent, "domainEvent");

            var type = domainEvent.GetType();

            if (!HandlesType(type))
            {
                return;
            }

            var method = _eventHandlers[type].GetType().GetMethod("ProcessEvent", new[] { type });

            if (method == null)
            {
                throw new ProcessEventMethodMissingException(string.Format(
                    RecallResources.ProcessEventMethodMissingException,
                    _eventHandlers[type].GetType().FullName,
                    type.FullName));
            }

            method.Invoke(_eventHandlers[type], new[] { domainEvent });
        }
    }
}