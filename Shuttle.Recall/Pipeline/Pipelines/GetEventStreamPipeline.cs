using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public interface IGetEventStreamPipeline : IPipeline
{
    Task<EventStream> ExecuteAsync(Guid id, EventStreamBuilder builder);
}

public class GetEventStreamPipeline : Pipeline, IGetEventStreamPipeline
{
    public GetEventStreamPipeline(IOptions<PipelineOptions> pipelineOptions, IServiceProvider serviceProvider, IRetrieveStreamEventsObserver retrieveStreamEventsObserver, IAssembleEventStreamObserver assembleEventStreamObserver)
        : base(pipelineOptions, serviceProvider)
    {
        AddStage("GetEventStream")
            .WithEvent<RetrieveStreamEvents>()
            .WithEvent<StreamEventsRetrieved>()
            .WithEvent<AssembleEventStream>()
            .WithEvent<EventStreamAssembled>();

        AddObserver(Guard.AgainstNull(retrieveStreamEventsObserver));
        AddObserver(Guard.AgainstNull(assembleEventStreamObserver));
    }

    public async Task<EventStream> ExecuteAsync(Guid id, EventStreamBuilder builder)
    {
        State.SetId(id);
        State.SetEventStreamBuilder(builder);

        await ExecuteAsync().ConfigureAwait(false);

        return State.GetEventStream();
    }
}