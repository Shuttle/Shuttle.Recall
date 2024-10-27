using System;
using System.Threading.Tasks;

namespace Shuttle.Recall;

public interface IEventProcessor : IDisposable, IAsyncDisposable
{
    bool Started { get; }
    Task<Projection?> AddProjectionAsync(string name);
    Projection GetProjection(string name);
    Projection? GetProjection();

    ProjectionAggregation GetProjectionAggregation(Guid id);
    void ReleaseProjection(Projection projection);
    Task<IEventProcessor> StartAsync();
    Task StopAsync();
}