using System;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public class EventStoreBuilder
{
    public event EventHandler<AddPipelineProcessingEventArgs>? AddPipelineProcessing;

    private EventStoreOptions _eventStoreOptions = new();

    public EventStoreBuilder(IServiceCollection services)
    {
        Services = Guard.AgainstNull(services);
    }

    public IEventProcessorConfiguration EventProcessorConfiguration { get; } = new EventProcessorConfiguration();

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
    public bool ShouldSuppressPipelineProcessing { get; private set; }

    public EventStoreBuilder SuppressPipelineTransactionScope()
    {
        ShouldSuppressPipelineTransactionScope = true;

        return this;
    }

    public EventStoreBuilder SuppressPipelineProcessing()
    {
        ShouldSuppressPipelineProcessing = true;

        return this;
    }

    public ProjectionBuilder AddProjection(string name)
    {
        return new(Services, EventProcessorConfiguration, name);
    }

    public void OnAddPipelineProcessing(PipelineProcessingBuilder pipelineProcessingBuilder)
    {
        AddPipelineProcessing?.Invoke(this, new(pipelineProcessingBuilder));
    }
}