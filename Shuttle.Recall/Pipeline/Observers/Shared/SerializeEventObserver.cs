using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Serialization;
using Shuttle.Core.Streams;

namespace Shuttle.Recall;

public interface ISerializeEventObserver : IPipelineObserver<OnSerializeEvent>
{
}

public class SerializeEventObserver : ISerializeEventObserver
{
    private readonly ISerializer _serializer;

    public SerializeEventObserver(ISerializer serializer)
    {
        _serializer = Guard.AgainstNull(serializer);
    }

    public async Task ExecuteAsync(IPipelineContext<OnSerializeEvent> pipelineContext)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var domainEvent = state.GetDomainEvent();
        var bytes = await (await _serializer.SerializeAsync(domainEvent.Event).ConfigureAwait(false)).ToBytesAsync().ConfigureAwait(false);

        state.SetEventBytes(bytes);
    }
}