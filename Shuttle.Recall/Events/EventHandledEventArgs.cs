using Shuttle.Contract;
using Shuttle.Pipelines;

namespace Shuttle.Recall;

public class EventHandledEventArgs(ProjectionEvent projectionEvent, EventEnvelope eventEnvelope, IPipeline pipeline) : PipelineEventArgs(pipeline)
{
    public EventEnvelope EventEnvelope { get; } = Guard.AgainstNull(eventEnvelope);
    public ProjectionEvent ProjectionEvent { get; } = Guard.AgainstNull(projectionEvent);
}
