using System.Threading;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Recall;

public class EventProcessor : IEventProcessor
{
    private readonly IPipelineFactory _pipelineFactory;

    private CancellationToken _cancellationToken;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private IProcessorThreadPool? _eventProcessorThreadPool;

    public EventProcessor(IPipelineFactory pipelineFactory)
    {
        _pipelineFactory = Guard.AgainstNull(pipelineFactory);
    }

    public void Dispose()
    {
        StopAsync().GetAwaiter().GetResult();
    }

    public bool Started { get; private set; }

    public async Task StopAsync()
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
        await StopAsync().ConfigureAwait(false);
    }

    public async Task<IEventProcessor> StartAsync()
    {
        if (Started)
        {
            return this;
        }

        var startupPipeline = _pipelineFactory.GetPipeline<EventProcessorStartupPipeline>();

        await startupPipeline.ExecuteAsync(_cancellationToken).ConfigureAwait(false);

        _cancellationToken = _cancellationTokenSource.Token;

        _eventProcessorThreadPool = startupPipeline.State.Get<IProcessorThreadPool>("EventProcessorThreadPool");

        Started = true;

        return this;
    }
}