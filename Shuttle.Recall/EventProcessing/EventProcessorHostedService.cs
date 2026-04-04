using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shuttle.Core.Contract;

namespace Shuttle.Recall;

public class EventProcessorHostedService(IEventProcessorConfiguration eventProcessorConfiguration, IServiceScopeFactory serviceScopeFactory) : IHostedService
{
    private IEventProcessor? _eventProcessor;
    private IServiceScope? _serviceScope;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!Guard.AgainstNull(eventProcessorConfiguration).HasProjections)
        {
            return;
        }

        _serviceScope = Guard.AgainstNull(serviceScopeFactory).CreateScope();

        _eventProcessor = await _serviceScope.ServiceProvider.GetRequiredService<IEventProcessor>().StartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (!Guard.AgainstNull(eventProcessorConfiguration).HasProjections)
        {
            return;
        }

        if (_eventProcessor != null)
        {
            await _eventProcessor.DisposeAsync();
        }

        _serviceScope?.Dispose();
    }
}