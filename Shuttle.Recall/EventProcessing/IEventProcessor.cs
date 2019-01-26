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
        Projection GetProjection();
        void ReleaseProjection(string name);

        ProjectionAggregation GetProjectionAggregation(Guid id);
    }
}