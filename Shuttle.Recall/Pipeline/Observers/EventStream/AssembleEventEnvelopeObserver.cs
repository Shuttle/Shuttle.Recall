using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public interface IAssembleEventEnvelopeObserver : IPipelineObserver<AssembleEventEnvelope>;

public class AssembleEventEnvelopeObserver(IOptions<RecallOptions> recallOptions) : IAssembleEventEnvelopeObserver
{
    private readonly RecallOptions _recallOptions = Guard.AgainstNull(Guard.AgainstNull(recallOptions).Value);

    public async Task ExecuteAsync(IPipelineContext<AssembleEventEnvelope> pipelineContext, CancellationToken cancellationToken = default)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var domainEvent = state.GetDomainEvent();
        var domainEventType = Guard.AgainstNull(domainEvent.Event.GetType());
        var builder = state.GetEventStreamBuilder();

        var eventEnvelope = new EventEnvelope
        {
            Event = state.GetEventBytes(),
            EventType = Guard.AgainstEmpty(domainEventType.FullName),
            Version = domainEvent.Version,
            AssemblyQualifiedName = Guard.AgainstEmpty(domainEventType.AssemblyQualifiedName),
            EncryptionAlgorithm = _recallOptions.EventStore.EncryptionAlgorithm,
            CompressionAlgorithm = _recallOptions.EventStore.CompressionAlgorithm,
            Headers = builder.Headers
        };

        state.SetEventEnvelope(eventEnvelope);

        await Task.CompletedTask;
    }
}