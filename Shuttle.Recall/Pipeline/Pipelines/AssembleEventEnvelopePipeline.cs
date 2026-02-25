using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.TransactionScope;

namespace Shuttle.Recall;

public interface IAssembleEventEnvelopePipeline : IPipeline
{
    Task<EventEnvelope> ExecuteAsync(DomainEvent domainEvent);
}

public class AssembleEventEnvelopePipeline : Pipeline, IAssembleEventEnvelopePipeline
{
    public AssembleEventEnvelopePipeline(IOptions<PipelineOptions> pipelineOptions, IOptions<TransactionScopeOptions> transactionScopeOptions, ITransactionScopeFactory transactionScopeFactory, IServiceProvider serviceProvider)
        : base(pipelineOptions, transactionScopeOptions, transactionScopeFactory, serviceProvider)
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