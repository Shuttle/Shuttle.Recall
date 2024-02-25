using System;
using System.Threading.Tasks;

namespace Shuttle.Recall
{
    public interface IEventProcessor : IDisposable, IAsyncDisposable
    {
        bool Started { get; }
        bool Asynchronous { get; }
        IEventProcessor Start();
        Projection AddProjection(string name);
        Projection GetProjection(string name);
        Projection GetProjection();
        void ReleaseProjection(Projection projection);

        ProjectionAggregation GetProjectionAggregation(Guid id);
        Task<IEventProcessor> StartAsync();
        Task StopAsync();
    }
}