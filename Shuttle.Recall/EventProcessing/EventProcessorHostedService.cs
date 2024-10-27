using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Shuttle.Core.Contract;

namespace Shuttle.Recall;

public class EventProcessorHostedService : IHostedService
{
    private readonly IEventProcessor _eventProcessor;

    public EventProcessorHostedService(IEventProcessor eventProcessor)
    {
        _eventProcessor = Guard.AgainstNull(eventProcessor);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _eventProcessor.StartAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _eventProcessor.StopAsync();
    }
}