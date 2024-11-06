using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Recall;

public class EventStoreBuilder
{
    private EventStoreOptions _eventStoreOptions = new();
    private ProjectionConfiguration _projectionConfiguration = new();

    public EventStoreBuilder(IServiceCollection services)
    {
        Services = Guard.AgainstNull(services);
    }

    public ProjectionConfiguration Configuration
    {
        get => _projectionConfiguration;
        set => _projectionConfiguration = Guard.AgainstNull(value);
    }

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
        return _projectionConfiguration.AddProjectionBuilder(new ProjectionBuilder(name));
    }

    public EventStoreBuilder AddEventHandler<TEventHandler>(string projectionName) where TEventHandler : class
    {
        Guard.AgainstNullOrEmptyString(projectionName);

        var type = typeof(TEventHandler);

        Services.AddTransient(type, type);

        Configuration.AddProjectionEventHandlerType(projectionName, type);

        return this;
    }

    public EventStoreBuilder AddEventHandler<TEventHandler>(Projection projection) where TEventHandler : class
    {
        Guard.AgainstNull(projection);

        var type = typeof(TEventHandler);

        Services.AddTransient(type, type);

        Configuration.AddProjectionEventHandlerType(projection.Name, type);

        return this;
    }
}