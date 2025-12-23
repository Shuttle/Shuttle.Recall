using Microsoft.Extensions.Hosting;
using Shuttle.Core.Contract;

namespace Shuttle.Recall;

public class EventProcessorHostedService(IEventProcessor eventProcessor) : IHostedService
{
    private readonly IEventProcessor _eventProcessor = Guard.AgainstNull(eventProcessor);

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _eventProcessor.StartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _eventProcessor.StopAsync(cancellationToken);
    }
}