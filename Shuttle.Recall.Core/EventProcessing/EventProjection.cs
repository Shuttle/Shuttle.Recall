using System;
using System.Collections.Generic;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Core
{
	public class EventProjection : IEventProjection
	{
		private static readonly Type EventHandlerType = typeof (IEventHandler<>);
		private readonly Dictionary<Type, object> _eventHandlers = new Dictionary<Type, object>();
		private readonly List<Type> _explicitTypes = new List<Type>();

		public EventProjection(string name)
		{
			Guard.AgainstNullOrEmptyString(name, "name");

			Name = name;
		}

		public string Name { get; private set; }

		public bool HasExplicitTypes
		{
			get { return _explicitTypes.Count > 0; }
		}

		public IEnumerable<Type> ExplicitTypes
		{
			get { return _explicitTypes; }
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

		public void Process(ProjectionEvent projectionEvent, IThreadState threadState)
		{
			Guard.AgainstNull(projectionEvent, "ProjectionEvent");
			Guard.AgainstNull(threadState, "threadState");

			var domainEventType = projectionEvent.Event.Data.GetType();

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

			var handlerContext = Activator.CreateInstance(contextType, projectionEvent, projectionEvent.Event.Data, threadState);

			method.Invoke(_eventHandlers[domainEventType], new[] {handlerContext});
		}

		public void AddExplicitType(Type type)
		{
			Guard.AgainstNull(type, "type");

			_explicitTypes.Add(type);
		}
	}
}