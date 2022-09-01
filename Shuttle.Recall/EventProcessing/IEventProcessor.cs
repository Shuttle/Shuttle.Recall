using System;

namespace Shuttle.Recall
{
    public interface IEventProcessor : IDisposable
    {
        bool Started { get; }
        IEventProcessor Start();
        void Stop();
        Projection AddProjection(string name);
        Projection GetProjection(string name);
        Projection GetProjection();
        void ReleaseProjection(Projection projection);

        ProjectionAggregation GetProjectionAggregation(Guid id);
    }
}