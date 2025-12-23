using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Recall;

public class ProjectionProcessorFactory(EventStoreOptions eventStoreOptions, IPipelineFactory pipelineFactory)
    : IProcessorFactory
{
    private readonly EventStoreOptions _eventStoreOptions = Guard.AgainstNull(eventStoreOptions);
    private readonly IPipelineFactory _pipelineFactory = Guard.AgainstNull(pipelineFactory);

    public Task<IProcessor> CreateAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IProcessor>(new ProjectionProcessor(_eventStoreOptions, _pipelineFactory));
    }
}