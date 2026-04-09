using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Shuttle.Contract;
using Shuttle.Pipelines;
using Shuttle.Threading;

namespace Shuttle.Recall;

public class EventProcessor(IServiceScopeFactory serviceScopeFactory, IOptions<RecallOptions> recallOptions, ILogger<EventProcessor>? logger = null) : IEventProcessor
{
    private readonly ILogger<EventProcessor> _logger = logger ?? NullLogger<EventProcessor>.Instance;
    private readonly RecallOptions _recallOptions = Guard.AgainstNull(Guard.AgainstNull(recallOptions).Value);
    private CancellationTokenSource _cancellationTokenSource = new();

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

        LogMessage.EventProcessorStop(_logger);

        await _recallOptions.Operation.InvokeAsync(new("[StopAsync]"), cancellationToken);

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

        LogMessage.EventProcessorStart(_logger);

        await _recallOptions.Operation.InvokeAsync(new("[StartAsync]"), cancellationToken);

        _serviceScope = Guard.AgainstNull(serviceScopeFactory).CreateScope();
        _cancellationTokenSource = new();

        var startupPipeline = _serviceScope.ServiceProvider.GetRequiredService<IEventProcessorStartupPipeline>();

        await startupPipeline.ExecuteAsync(_cancellationTokenSource.Token).ConfigureAwait(false);

        _projectionProcessorThreadPool = startupPipeline.State.Get<IProcessorThreadPool>("ProjectionProcessorThreadPool");

        Started = true;

        return this;
    }
}