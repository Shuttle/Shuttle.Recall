using System;
using System.Collections.Generic;

namespace Shuttle.Recall
{
    public interface IEventProcessor : IDisposable
    {
        bool Started { get; }
        IEventProcessor Start();
        void Stop();
        void AddProjection(Projection projection);
        Projection GetProjection(string name);
        Projection GetProjection();
        void ReleaseProjection(Projection projection);

        ProjectionAggregation GetProjectionAggregation(Guid id);
    }
}