using System;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public class GetEventStreamPipeline : Pipeline
{
    public GetEventStreamPipeline(IServiceProvider serviceProvider, IGetStreamEventsObserver getStreamEventsObserver, IAssembleEventStreamObserver assembleEventStreamObserver) 
        : base(serviceProvider)
    {
        RegisterStage("GetEventStream")
            .WithEvent<OnBeforeGetStreamEvents>()
            .WithEvent<OnGetStreamEvents>()
            .WithEvent<OnAfterGetStreamEvents>()
            .WithEvent<OnAssembleEventStream>()
            .WithEvent<OnAfterAssembleEventStream>();

        RegisterObserver(Guard.AgainstNull(getStreamEventsObserver));
        RegisterObserver(Guard.AgainstNull(assembleEventStreamObserver));
    }

    public async Task<EventStream> ExecuteAsync(Guid id, EventStreamBuilder builder)
    {
        State.SetId(id);
        State.SetEventStreamBuilder(builder);

        await ExecuteAsync().ConfigureAwait(false);

        return State.GetEventStream();
    }
}