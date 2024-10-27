using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public interface IAssembleEventEnvelopeObserver : IPipelineObserver<OnAssembleEventEnvelope>
{
}

public class AssembleEventEnvelopeObserver : IAssembleEventEnvelopeObserver
{
    private readonly EventStoreOptions _eventStoreOptions;

    public AssembleEventEnvelopeObserver(IOptions<EventStoreOptions> eventStoreOptions)
    {
        _eventStoreOptions = Guard.AgainstNull(Guard.AgainstNull(eventStoreOptions).Value);
    }

    public async Task ExecuteAsync(IPipelineContext<OnAssembleEventEnvelope> pipelineContext)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var domainEvent = Guard.AgainstNull(state.GetDomainEvent());
        var domainEventType = domainEvent.Event.GetType();
        var builder = state.GetEventStreamBuilder();

        var eventEnvelope = new EventEnvelope
        {
            Event = Guard.AgainstNull(state.GetEventBytes()),
            EventType = Guard.AgainstNullOrEmptyString(domainEventType.FullName),
            IsSnapshot = domainEvent.IsSnapshot,
            Version = domainEvent.Version,
            AssemblyQualifiedName = Guard.AgainstNullOrEmptyString(domainEventType.AssemblyQualifiedName),
            EncryptionAlgorithm = _eventStoreOptions.EncryptionAlgorithm,
            CompressionAlgorithm = _eventStoreOptions.CompressionAlgorithm
        };

        if (builder != null)
        {
            eventEnvelope.Headers = builder.Headers;
        }

        state.SetEventEnvelope(eventEnvelope);

        await Task.CompletedTask;
    }
}