using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shuttle.Contract;

namespace Shuttle.Recall;

public class EventProcessorHostedService(IEventProcessorConfiguration eventProcessorConfiguration, IOptions<RecallOptions> recallOptions, IServiceScopeFactory serviceScopeFactory) : IHostedService
{
    private IEventProcessor? _eventProcessor;
    private IServiceScope? _serviceScope;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!Guard.AgainstNull(eventProcessorConfiguration).HasProjections || !Guard.AgainstNull(recallOptions).Value.EventProcessing.AutoStart)
        {
            return;
        }

        _serviceScope = Guard.AgainstNull(serviceScopeFactory).CreateScope();

        _eventProcessor = await _serviceScope.ServiceProvider.GetRequiredService<IEventProcessor>().StartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (!Guard.AgainstNull(eventProcessorConfiguration).HasProjections || !Guard.AgainstNull(recallOptions).Value.EventProcessing.AutoStart)
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