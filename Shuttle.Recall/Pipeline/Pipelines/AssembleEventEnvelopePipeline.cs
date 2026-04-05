using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public interface IAssembleEventEnvelopePipeline : IPipeline
{
    Task<EventEnvelope> ExecuteAsync(DomainEvent domainEvent);
}

public class AssembleEventEnvelopePipeline : Pipeline, IAssembleEventEnvelopePipeline
{
    public AssembleEventEnvelopePipeline(IOptions<PipelineOptions> pipelineOptions, IServiceProvider serviceProvider)
        : base(pipelineOptions, serviceProvider)
    {
        AddStage("Get")
            .WithEvent<SerializeEvent>()
            .WithEvent<EventSerialized>()
            .WithEvent<AssembleEventEnvelope>()
            .WithEvent<EventEnvelopeAssembled>();

        AddObserver<ISerializeEventObserver>();
        AddObserver<IAssembleEventEnvelopeObserver>();
    }

    public async Task<EventEnvelope> ExecuteAsync(DomainEvent domainEvent)
    {
        State.SetDomainEvent(Guard.AgainstNull(domainEvent));

        await ExecuteAsync().ConfigureAwait(false);

        return State.GetEventEnvelope();
    }
}