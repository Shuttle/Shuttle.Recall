using Shuttle.Pipelines;

namespace Shuttle.Recall;

public interface IEventHandlerInvoker
{
    ValueTask<bool> InvokeAsync(IPipelineContext<HandleEvent> pipelineContext, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invokes the handler registered for <paramref name="projection"/> against a just-saved event, outside of the
    /// sequential event-processing pipeline. Unlike <see cref="InvokeAsync"/>, this does not require
    /// <see cref="PrimitiveEvent.SequenceNumber"/> to be set (it will not be, at this point) and does not advance
    /// the projection's checkpoint. Returns <see langword="true"/> only if the event was handled and the handler
    /// did not request a deferral.
    /// </summary>
    ValueTask<bool> InvokeImmediateAsync(Projection projection, EventEnvelope eventEnvelope, object domainEvent, PrimitiveEvent primitiveEvent, IServiceProvider serviceProvider, CancellationToken cancellationToken = default);
}