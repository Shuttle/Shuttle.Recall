using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Recall;

public class EventStoreBuilder
{
    private EventStoreOptions _eventStoreOptions = new();

    public EventStoreBuilder(IServiceCollection services)
    {
        Services = Guard.AgainstNull(services);
    }

    public IEventProcessorConfiguration EventProcessorConfiguration { get; private set; } = new EventProcessorConfiguration();

    public EventStoreOptions Options
    {
        get => _eventStoreOptions;
        set => _eventStoreOptions = Guard.AgainstNull(value);
    }

    public IServiceCollection Services { get; }
    public bool ShouldSuppressEventProcessorHostedService { get; private set; }

    public EventStoreBuilder SuppressEventProcessorHostedService()
    {
        ShouldSuppressEventProcessorHostedService = true;

        return this;
    }

    public bool ShouldSuppressPipelineTransactionScope { get; private set; }

    public EventStoreBuilder SuppressPipelineTransactionScope()
    {
        ShouldSuppressPipelineTransactionScope = true;

        return this;
    }

    public ProjectionBuilder AddProjection(string name)
    {
        return new(Services, EventProcessorConfiguration, name);
    }
}