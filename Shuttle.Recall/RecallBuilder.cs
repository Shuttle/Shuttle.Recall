using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Recall;

public class RecallBuilder(IServiceCollection services, IEventProcessorConfiguration eventProcessorConfiguration)
{
    private readonly List<Action<RecallOptions>> _configureActions = [];

    public IEventProcessorConfiguration EventProcessorConfiguration { get; } = Guard.AgainstNull(eventProcessorConfiguration);

    public IServiceCollection Services { get; } = Guard.AgainstNull(services);
    public bool ShouldSuppressPrimitiveEventSequencerHostedService { get; private set; }
    public bool ShouldSuppressEventProcessorHostedService { get; private set; }
    public bool ShouldSuppressPipelineProcessing { get; private set; }

    public RecallBuilder Configure(Action<RecallOptions> configureOptions)
    {
        _configureActions.Add(Guard.AgainstNull(configureOptions));
        return this;
    }

    public ProjectionBuilder AddProjection(string name)
    {
        return new(Services, EventProcessorConfiguration, name);
    }

    public RecallBuilder SuppressEventProcessorHostedService()
    {
        ShouldSuppressEventProcessorHostedService = true;

        return this;
    }

    public RecallBuilder SuppressPipelineProcessing()
    {
        ShouldSuppressPipelineProcessing = true;

        return this;
    }

    public RecallBuilder SuppressPrimitiveEventSequencerHostedService()
    {
        ShouldSuppressPrimitiveEventSequencerHostedService = true;
        
        return this;
    }
}