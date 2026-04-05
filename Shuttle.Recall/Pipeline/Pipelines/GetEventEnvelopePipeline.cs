using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public interface IGetEventEnvelopePipeline : IPipeline
{
    Task ExecuteAsync(PrimitiveEvent? primitiveEvent);
}

public class GetEventEnvelopePipeline : Pipeline, IGetEventEnvelopePipeline
{
    public GetEventEnvelopePipeline(IOptions<PipelineOptions> pipelineOptions, IServiceProvider serviceProvider)
        : base(pipelineOptions, serviceProvider)
    {
        AddStage("GetEventEnvelope")
            .WithEvent<DeserializeEventEnvelope>()
            .WithEvent<EventEnvelopeDeserialized>()
            .WithEvent<DeserializeEvent>()
            .WithEvent<EventDeserialized>();

        AddObserver<IDeserializeEventEnvelopeObserver>();
        AddObserver<IDeserializeEventObserver>();
    }

    public async Task ExecuteAsync(PrimitiveEvent? primitiveEvent)
    {
        Guard.AgainstNull(primitiveEvent);

        State.SetPrimitiveEvent(primitiveEvent);

        await ExecuteAsync().ConfigureAwait(false);
    }
}