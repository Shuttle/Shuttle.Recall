using System;
using System.Collections.Generic;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public static class PipelineStateExtensions
{
    public static DomainEvent GetDomainEvent(this IState state)
    {
        return Guard.AgainstNull(state.Get<DomainEvent>(StateKeys.DomainEvent));
    }

    public static byte[] GetEventBytes(this IState state)
    {
        return Guard.AgainstNull(state.Get<byte[]>(StateKeys.EventBytes));
    }

    public static EventEnvelope GetEventEnvelope(this IState state)
    {
        return Guard.AgainstNull(state.Get<EventEnvelope>(StateKeys.EventEnvelope));
    }

    public static IEnumerable<EventEnvelope>? GetEventEnvelopes(this IState state)
    {
        return state.Get<IEnumerable<EventEnvelope>>(StateKeys.EventEnvelopes);
    }

    public static IEnumerable<DomainEvent> GetEvents(this IState state)
    {
        return Guard.AgainstNull(state.Get<IEnumerable<DomainEvent>>(StateKeys.Events));
    }

    public static EventStream GetEventStream(this IState state)
    {
        return Guard.AgainstNull(state.Get<EventStream>(StateKeys.EventStream));
    }

    public static EventStreamBuilder GetEventStreamBuilder(this IState state)
    {
        return Guard.AgainstNull(state.Get<EventStreamBuilder>(StateKeys.EventStreamBuilder));
    }

    public static Guid GetId(this IState state)
    {
        return state.Get<Guid>(StateKeys.Id);
    }

    public static PrimitiveEvent GetPrimitiveEvent(this IState state)
    {
        return Guard.AgainstNull(state.Get<PrimitiveEvent>(StateKeys.PrimitiveEvent));
    }

    public static int GetProcessorThreadManagedThreadId(this IState state)
    {
        return state.Get<int>(StateKeys.ProcessorThreadManagedThreadId);
    }

    public static ProjectionEvent GetProjectionEvent(this IState state)
    {
        return Guard.AgainstNull(state.Get<ProjectionEvent>(StateKeys.ProjectionEvent));
    }

    public static long GetSequenceNumber(this IState state)
    {
        return state.Get<long>(StateKeys.SequenceNumber);
    }

    public static int GetVersion(this IState state)
    {
        return state.Get<int>(StateKeys.Version);
    }

    public static bool GetWorking(this IState state)
    {
        return state.Contains(StateKeys.Working) && state.Get<bool>(StateKeys.Working);
    }

    public static void SetDomainEvent(this IState state, DomainEvent domainEvent)
    {
        state.Replace(StateKeys.DomainEvent, domainEvent);
    }

    public static void SetEventBytes(this IState state, byte[] bytes)
    {
        state.Replace(StateKeys.EventBytes, Guard.AgainstNull(bytes));
    }

    public static void SetEventEnvelope(this IState state, EventEnvelope value)
    {
        state.Replace(StateKeys.EventEnvelope, value);
    }

    public static void SetEventEnvelopes(this IState state, IEnumerable<EventEnvelope> value)
    {
        state.Replace(StateKeys.EventEnvelopes, value);
    }

    public static void SetEvents(this IState state, IEnumerable<DomainEvent> value)
    {
        state.Replace(StateKeys.Events, value);
    }

    public static void SetEventStream(this IState state, EventStream value)
    {
        state.Replace(StateKeys.EventStream, value);
    }

    public static void SetEventStreamBuilder(this IState state, EventStreamBuilder value)
    {
        state.Replace(StateKeys.EventStreamBuilder, value);
    }

    public static void SetId(this IState state, Guid id)
    {
        state.Replace(StateKeys.Id, id);
    }

    public static void SetPrimitiveEvent(this IState state, PrimitiveEvent? primitiveEvent)
    {
        state.Replace(StateKeys.PrimitiveEvent, primitiveEvent);
    }

    public static void SetProcessorThreadManagedThreadId(this IState state, int processorThreadManagedThreadId)
    {
        state.Replace(StateKeys.ProcessorThreadManagedThreadId, processorThreadManagedThreadId);
    }

    public static void SetProjectionEvent(this IState state, ProjectionEvent projectionEvent)
    {
        state.Replace(StateKeys.ProjectionEvent, projectionEvent);
    }

    public static void SetSequenceNumber(this IState state, long sequenceNumber)
    {
        state.Replace(StateKeys.SequenceNumber, sequenceNumber);
    }

    public static void SetVersion(this IState state, int value)
    {
        state.Replace(StateKeys.Version, value);
    }

    public static void SetWorking(this IState state)
    {
        state.Replace(StateKeys.Working, true);
    }
}