using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Core
{
    public interface IEventProjection
    {
        string Name { get; }
        bool HandlesType(Type type);
        EventProjection AddEventHandler(object handler);
        void Process(ProjectionEvent projectionEvent, IThreadState threadState);
    }
}