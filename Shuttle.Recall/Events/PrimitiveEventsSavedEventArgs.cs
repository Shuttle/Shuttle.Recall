using Shuttle.Contract;
using Shuttle.Pipelines;

namespace Shuttle.Recall;

public class PrimitiveEventsSavedEventArgs(IEnumerable<PrimitiveEvent> primitiveEvents, IPipeline pipeline) : PipelineEventArgs(pipeline)
{
    public IEnumerable<PrimitiveEvent> PrimitiveEvents { get; } = Guard.AgainstNull(primitiveEvents);
}