using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Recall;

public class EventProcessor(IServiceScopeFactory serviceScopeFactory) : IEventProcessor
{
    private CancellationTokenSource _cancellationTokenSource = new();
    private IPipelineFactory? _pipelineFactory;

    private IProcessorThreadPool? _projectionProcessorThreadPool;
    private IServiceScope? _serviceScope;

    public void Dispose()
    {
        StopAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    public bool Started { get; private set; }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!Started)
        {
            return;
        }

        await _cancellationTokenSource.CancelAsync();

        _projectionProcessorThreadPool?.Dispose();

        _serviceScope?.Dispose();

        Started = false;

        await Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync(CancellationToken.None).ConfigureAwait(false);
    }

    public async Task<IEventProcessor> StartAsync(CancellationToken cancellationToken = default)
    {
        if (Started)
        {
            return this;
        }

        _serviceScope = Guard.AgainstNull(serviceScopeFactory).CreateScope();
        _pipelineFactory = _serviceScope.ServiceProvider.GetRequiredService<IPipelineFactory>();
        _cancellationTokenSource = new();

        var startupPipeline = await _pipelineFactory.GetPipelineAsync<EventProcessorStartupPipeline>(cancellationToken);

        await startupPipeline.ExecuteAsync(_cancellationTokenSource.Token).ConfigureAwait(false);

        _projectionProcessorThreadPool = startupPipeline.State.Get<IProcessorThreadPool>("ProjectionProcessorThreadPool");

        Started = true;

        return this;
    }
}