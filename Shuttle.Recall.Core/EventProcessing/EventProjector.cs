using System;
using System.CodeDom;
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

            var typesAddedCount = 0;

            foreach (var interfaceType in handler.GetType().InterfacesAssignableTo(EventHandlerType))
            {
                var type = interfaceType.GetGenericArguments()[0];

                _eventHandlers.Add(type, handler);

                typesAddedCount++;
            }

            if (typesAddedCount == 0)
            {
                throw new EventProcessingException(string.Format(RecallResources.InvalidEventHandlerType, handler.GetType().FullName));
            }

            return this;
        }

        public void Process(EventRead eventRead, IThreadState threadState)
        {
            Guard.AgainstNull(eventRead, "eventRead");
            Guard.AgainstNull(threadState, "threadState");

            var domainEventType = eventRead.Event.Data.GetType();

            if (!HandlesType(domainEventType))
            {
                return;
            }

            var contextType = typeof(EventHandlerContext<>).MakeGenericType(domainEventType);
            var method = _eventHandlers[domainEventType].GetType().GetMethod("ProcessEvent", new[] { contextType });

            if (method == null)
            {
                throw new ProcessEventMethodMissingException(string.Format(
                    RecallResources.ProcessEventMethodMissingException,
                    _eventHandlers[domainEventType].GetType().FullName,
                    domainEventType.FullName));
            }

            var handlerContext = Activator.CreateInstance(contextType, eventRead, eventRead.Event.Data, threadState);

            method.Invoke(_eventHandlers[domainEventType], new object[] { handlerContext });
        }
    }
}