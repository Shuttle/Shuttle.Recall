using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Recall;

public class EventProcessor(IPipelineFactory pipelineFactory) : IEventProcessor
{
    private CancellationTokenSource _cancellationTokenSource = new();
    private readonly IPipelineFactory _pipelineFactory = Guard.AgainstNull(pipelineFactory);

    private IProcessorThreadPool? _eventProcessorThreadPool;

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

        _eventProcessorThreadPool?.Dispose();

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

        _cancellationTokenSource = new();

        var startupPipeline = await _pipelineFactory.GetPipelineAsync<EventProcessorStartupPipeline>(cancellationToken);

        await startupPipeline.ExecuteAsync(_cancellationTokenSource.Token).ConfigureAwait(false);

        _eventProcessorThreadPool = startupPipeline.State.Get<IProcessorThreadPool>("EventProcessorThreadPool");

        Started = true;

        return this;
    }
}