using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public interface IRemoveEventStreamPipeline : IPipeline
{
    Task ExecuteAsync(Guid id);
}

public class RemoveEventStreamPipeline : Pipeline, IRemoveEventStreamPipeline
{
    public RemoveEventStreamPipeline(IOptions<PipelineOptions> pipelineOptions, IServiceProvider serviceProvider, IRemoveEventStreamObserver removeEventStreamObserver)
        : base(pipelineOptions, serviceProvider)
    {
        AddStage("RemoveEventStream")
            .WithEvent<RemoveEventStream>()
            .WithEvent<EventStreamRemoved>();

        AddObserver(Guard.AgainstNull(removeEventStreamObserver));
    }

    public async Task ExecuteAsync(Guid id)
    {
        State.SetId(id);

        await ExecuteAsync().ConfigureAwait(false);
    }
}