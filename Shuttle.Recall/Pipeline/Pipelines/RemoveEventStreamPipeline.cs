using System;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public class RemoveEventStreamPipeline : Pipeline
{
    public RemoveEventStreamPipeline(IServiceProvider serviceProvider, IRemoveEventStreamObserver removeEventStreamObserver) 
        : base(serviceProvider)
    {
        RegisterStage("RemoveEventStream")
            .WithEvent<OnBeforeRemoveEventStream>()
            .WithEvent<OnRemoveEventStream>()
            .WithEvent<OnAfterRemoveEventStream>();

        RegisterObserver(Guard.AgainstNull(removeEventStreamObserver));
    }

    public async Task ExecuteAsync(Guid id)
    {
        State.SetId(id);

        await ExecuteAsync().ConfigureAwait(false);
    }
}