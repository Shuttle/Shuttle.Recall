using System;
using System.Collections.Generic;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
	public class EventProjection
	{
		private static readonly Type EventHandlerType = typeof (IEventHandler<>);
        private readonly Dictionary<Type, object> _eventHandlers = new Dictionary<Type, object>();

		public EventProjection(string name)
		{
			Guard.AgainstNullOrEmptyString(name, "name");

			Name = name;
		}

		public string Name { get; private set; }

		public IEnumerable<Type> EventTypes
		{
			get { return _eventHandlers.Keys; }
		}

		public bool HandlesType(Type type)
		{
			Guard.AgainstNull(type, "type");

			return _eventHandlers.ContainsKey(type);
		}

		public EventProjection AddEventHandler(object handler)
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
				throw new EventProcessingException(string.Format(RecallResources.InvalidEventHandlerType,
					handler.GetType().FullName));
			}

			return this;
		}

		public void Process(EventEnvelope eventEnvelope, long sequenceNumber, IThreadState threadState)
		{
			Guard.AgainstNull(eventEnvelope, "ProjectionEvent");
			Guard.AgainstNull(threadState, "threadState");

			var domainEventType = Type.GetType(eventEnvelope.AssemblyQualifiedName, true);

			if (!HandlesType(domainEventType))
			{
				return;
			}

			var contextType = typeof (EventHandlerContext<>).MakeGenericType(domainEventType);
			var method = _eventHandlers[domainEventType].GetType().GetMethod("ProcessEvent", new[] {contextType});

			if (method == null)
			{
				throw new ProcessEventMethodMissingException(string.Format(
					RecallResources.ProcessEventMethodMissingException,
					_eventHandlers[domainEventType].GetType().FullName,
					domainEventType.FullName));
			}

			var handlerContext = Activator.CreateInstance(contextType, eventEnvelope, eventEnvelope.Event, sequenceNumber, threadState);

			method.Invoke(_eventHandlers[domainEventType], new[] {handlerContext});
		}
	}
}