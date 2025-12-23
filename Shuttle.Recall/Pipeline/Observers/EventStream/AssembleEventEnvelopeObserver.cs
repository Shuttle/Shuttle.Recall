using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public interface IAssembleEventEnvelopeObserver : IPipelineObserver<AssembleEventEnvelope>;

public class AssembleEventEnvelopeObserver(IOptions<EventStoreOptions> eventStoreOptions) : IAssembleEventEnvelopeObserver
{
    private readonly EventStoreOptions _eventStoreOptions = Guard.AgainstNull(Guard.AgainstNull(eventStoreOptions).Value);

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
            EncryptionAlgorithm = _eventStoreOptions.EncryptionAlgorithm,
            CompressionAlgorithm = _eventStoreOptions.CompressionAlgorithm,
            Headers = builder.Headers
        };

        state.SetEventEnvelope(eventEnvelope);

        await Task.CompletedTask;
    }
}