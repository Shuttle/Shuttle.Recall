using System;

namespace Shuttle.Recall.Core
{
    public interface IEventProjector
    {
        string Name { get; }
        bool HandlesType(Type type);
        EventProjector AddEventHandler(object handler);
        void Process(object domainEvent);
    }
}