using Shuttle.Contract;
using Shuttle.Pipelines;

namespace Shuttle.Recall;

public class ImmediateConsistencyFailedEventArgs(string projectionName, PrimitiveEvent primitiveEvent, Exception exception, IPipeline pipeline) : PipelineEventArgs(pipeline)
{
    public Exception Exception { get; } = Guard.AgainstNull(exception);
    public PrimitiveEvent PrimitiveEvent { get; } = Guard.AgainstNull(primitiveEvent);
    public string ProjectionName { get; } = Guard.AgainstEmpty(projectionName);
}
