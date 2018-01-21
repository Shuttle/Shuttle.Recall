using System;

namespace Shuttle.Recall
{
    public interface IEventProcessor : IDisposable
    {
        bool Started { get; }
        IEventProcessor Start();
        void Stop();
        void AddProjection(Projection projection);
    }
}