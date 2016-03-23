using System;
using System.Collections.Generic;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
	public interface IEventProjection
	{
		string Name { get; }
		bool HasExplicitTypes { get; }
		IEnumerable<Type> ExplicitTypes { get; }
		bool HandlesType(Type type);
		EventProjection AddEventHandler(object handler);
		void Process(ProjectionEvent projectionEvent, IThreadState threadState);
		void AddExplicitType(Type type);
		void AddExplicitType<T>() where T : class;
	}
}