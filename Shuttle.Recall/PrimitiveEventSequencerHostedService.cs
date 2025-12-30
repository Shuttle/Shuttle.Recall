using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Threading;

namespace Shuttle.Recall;

public class PrimitiveEventSequencerHostedService(IOptions<ThreadingOptions> threadingOptions, IServiceScopeFactory serviceScopeFactory, IProcessorIdleStrategy processorIdleStrategy) : IHostedService
{
    private readonly IProcessorIdleStrategy _processorIdleStrategy = Guard.AgainstNull(processorIdleStrategy);
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly IServiceScopeFactory _serviceScopeFactory = Guard.AgainstNull(serviceScopeFactory);
    private readonly ThreadingOptions _threadingOptions = Guard.AgainstNull(Guard.AgainstNull(threadingOptions).Value);

    private ProcessorThread? _primitiveEventSequencerThread;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _primitiveEventSequencerThread = new("PrimitiveEventSequencerProcessor", _serviceScopeFactory, _threadingOptions, _processorIdleStrategy);

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