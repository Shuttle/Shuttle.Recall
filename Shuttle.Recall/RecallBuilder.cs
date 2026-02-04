using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Recall;

public class RecallBuilder(IServiceCollection services, IEventProcessorConfiguration eventProcessorConfiguration)
{
    public IEventProcessorConfiguration EventProcessorConfiguration { get; } = Guard.AgainstNull(eventProcessorConfiguration);

    public RecallOptions Options
    {
        get;
        set => field = Guard.AgainstNull(value);
    } = new();

    public IServiceCollection Services { get; } = Guard.AgainstNull(services);
    public bool ShouldSuppressPrimitiveEventSequencerHostedService { get; private set; }
    public bool ShouldSuppressEventProcessorHostedService { get; private set; }
    public bool ShouldSuppressPipelineProcessing { get; private set; }

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