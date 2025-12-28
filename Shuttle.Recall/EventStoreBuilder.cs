using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public class EventStoreBuilder(IServiceCollection services)
{
    public class AddPipelinesEventArgs(PipelineBuilder pipelineBuilder) : EventArgs
    {
        public PipelineBuilder PipelineBuilder { get; } = Guard.AgainstNull(pipelineBuilder);
    }

    public event EventHandler<AddPipelinesEventArgs>? AddPipelines;

    public IEventProcessorConfiguration EventProcessorConfiguration { get; } = new EventProcessorConfiguration();

    public EventStoreOptions Options
    {
        get;
        set => field = Guard.AgainstNull(value);
    } = new();

    public IServiceCollection Services { get; } = Guard.AgainstNull(services);
    public bool ShouldSuppressPrimitiveEventSequencerHostedService { get; private set; }
    public bool ShouldSuppressEventProcessorHostedService { get; private set; }
    public bool ShouldSuppressPipelineProcessing { get; private set; }
    public bool ShouldSuppressPipelineTransactionScope { get; private set; }

    public ProjectionBuilder AddProjection(string name)
    {
        return new(Services, EventProcessorConfiguration, name);
    }

    public EventStoreBuilder SuppressEventProcessorHostedService()
    {
        ShouldSuppressEventProcessorHostedService = true;

        return this;
    }

    public EventStoreBuilder SuppressPipelineProcessing()
    {
        ShouldSuppressPipelineProcessing = true;

        return this;
    }

    public EventStoreBuilder SuppressPipelineTransactionScope()
    {
        ShouldSuppressPipelineTransactionScope = true;

        return this;
    }

    public EventStoreBuilder SuppressPrimitiveEventSequencerHostedService()
    {
        ShouldSuppressPrimitiveEventSequencerHostedService = true;
        
        return this;
    }

    internal void OnAddPipelines(PipelineBuilder pipelineBuilder)
    {
        AddPipelines?.Invoke(this, new(pipelineBuilder));
    }
}