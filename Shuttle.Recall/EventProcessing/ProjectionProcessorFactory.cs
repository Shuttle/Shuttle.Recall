using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Recall;

public class ProjectionProcessorFactory : IProcessorFactory
{
    private readonly IEventProcessor _eventProcessor;
    private readonly EventStoreOptions _eventStoreOptions;
    private readonly IPipelineFactory _pipelineFactory;

    public ProjectionProcessorFactory(EventStoreOptions eventStoreOptions, IPipelineFactory pipelineFactory, IEventProcessor eventProcessor)
    {
        _eventStoreOptions = Guard.AgainstNull(eventStoreOptions);
        _pipelineFactory = Guard.AgainstNull(pipelineFactory);
        _eventProcessor = Guard.AgainstNull(eventProcessor);
    }

    public IProcessor Create()
    {
        return new ProjectionProcessor(_eventStoreOptions, _pipelineFactory);
    }
}