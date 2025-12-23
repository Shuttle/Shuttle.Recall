namespace Shuttle.Recall;

public interface IEventProcessor : IDisposable, IAsyncDisposable
{
    bool Started { get; }
    Task<IEventProcessor> StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}