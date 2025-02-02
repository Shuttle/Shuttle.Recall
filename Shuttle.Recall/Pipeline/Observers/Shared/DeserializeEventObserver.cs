using System;
using System.IO;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Serialization;

namespace Shuttle.Recall;

public interface IDeserializeEventObserver : IPipelineObserver<OnDeserializeEvent>
{
}

public class DeserializeEventObserver : IDeserializeEventObserver
{
    private readonly ISerializer _serializer;

    public DeserializeEventObserver(ISerializer serializer)
    {
        _serializer = Guard.AgainstNull(serializer);
    }

    public async Task ExecuteAsync(IPipelineContext<OnDeserializeEvent> pipelineContext)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var eventEnvelope = state.GetEventEnvelope();

        using (var stream = new MemoryStream(eventEnvelope.Event))
        {
            state.SetDomainEvent(new(await _serializer.DeserializeAsync(Guard.AgainstNull(Type.GetType(Guard.AgainstNullOrEmptyString(eventEnvelope.AssemblyQualifiedName), true, true)), stream), eventEnvelope.Version));
        }
    }
}