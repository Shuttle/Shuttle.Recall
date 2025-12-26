using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Threading;

namespace Shuttle.Recall;

public class PrimitiveEventSequencerHostedService(IOptions<EventStoreOptions> eventStoreOptions, IOptions<ThreadingOptions> threadingOptions, IServiceScopeFactory serviceScopeFactory, IPrimitiveEventSequencer primitiveEventSequencer) : IHostedService
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly EventStoreOptions _eventStoreOptions = Guard.AgainstNull(Guard.AgainstNull(eventStoreOptions).Value);
    private readonly IServiceScopeFactory _serviceScopeFactory = Guard.AgainstNull(serviceScopeFactory);
    private readonly ThreadingOptions _threadingOptions = Guard.AgainstNull(Guard.AgainstNull(threadingOptions).Value);

    private ProcessorThread? _primitiveEventSequencerThread;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _primitiveEventSequencerThread = new("PrimitiveEventSequencer", new PrimitiveEventSequencerProcessor(primitiveEventSequencer, new ThreadActivity(_eventStoreOptions.PrimitiveEventSequencerIdleDurations)), _serviceScopeFactory, _threadingOptions);

        await _primitiveEventSequencerThread.StartAsync(_cancellationTokenSource.Token);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _cancellationTokenSource.CancelAsync();

        if (_primitiveEventSequencerThread != null)
        {
            await _primitiveEventSequencerThread.StopAsync();
        }
    }
}