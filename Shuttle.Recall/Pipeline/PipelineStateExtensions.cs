using System;
using System.Collections.Generic;
using System.Threading;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall
{
    public static class PipelineStateExtensions
    {
        public static CancellationToken GetCancellationToken(this IState state)
        {
            return state.Get<CancellationToken>(StateKeys.CancellationToken);
        }

        public static void SetCancellationToken(this IState state, CancellationToken cancellationToken)
        {
            state.Replace(StateKeys.CancellationToken, cancellationToken);
        }

        public static void SetWorking(this IState state)
        {
            state.Replace(StateKeys.Working, true);
        }

        public static bool GetWorking(this IState state)
        {
            return state.Get<bool>(StateKeys.Working);
        }

        public static void SetEventEnvelope(this IState state, EventEnvelope value)
        {
            state.Replace(StateKeys.EventEnvelope, value);
        }

        public static EventEnvelope GetEventEnvelope(this IState state)
        {
            return state.Get<EventEnvelope>(StateKeys.EventEnvelope);
        }

        public static void SetEventEnvelopes(this IState state, IEnumerable<EventEnvelope> value)
        {
            state.Replace(StateKeys.EventEnvelopes, value);
        }

        public static IEnumerable<EventEnvelope> GetEventEnvelopes(this IState state)
        {
            return state.Get<IEnumerable<EventEnvelope>>(StateKeys.EventEnvelopes);
        }

        public static void SetEvent(this IState state, DomainEvent value)
        {
            state.Replace(StateKeys.Event, value);
        }

        public static DomainEvent GetEvent(this IState state)
        {
            return state.Get<DomainEvent>(StateKeys.Event);
        }

        public static void SetEventStream(this IState state, EventStream value)
        {
            state.Replace(StateKeys.EventStream, value);
        }

        public static EventStream GetEventStream(this IState state)
        {
            return state.Get<EventStream>(StateKeys.EventStream);
        }

        public static void SetEventStreamBuilder(this IState state, EventStreamBuilder value)
        {
            state.Replace(StateKeys.EventStreamBuilder, value);
        }

        public static EventStreamBuilder GetEventStreamBuilder(this IState state)
        {
            return state.Get<EventStreamBuilder>(StateKeys.EventStreamBuilder);
        }

        public static void SetEvents(this IState state, IEnumerable<DomainEvent> value)
        {
            state.Replace(StateKeys.Events, value);
        }

        public static IEnumerable<DomainEvent> GetEvents(this IState state)
        {
            return state.Get<IEnumerable<DomainEvent>>(StateKeys.Events);
        }

        public static void SetVersion(this IState state, int value)
        {
            state.Replace(StateKeys.Version, value);
        }

        public static int GetVersion(this IState state)
        {
            return state.Get<int>(StateKeys.Version);
        }

        public static byte[] GetEventBytes(this IState state)
        {
            return state.Get<byte[]>(StateKeys.EventBytes);
        }

        public static void SetEventBytes(this IState state, byte[] bytes)
        {
            state.Replace(StateKeys.EventBytes, bytes);
        }

        public static DomainEvent GetDomainEvent(this IState state)
        {
            return state.Get<DomainEvent>(StateKeys.DomainEvent);
        }

        public static void SetDomainEvent(this IState state, DomainEvent domainEvent)
        {
            state.Replace(StateKeys.DomainEvent, domainEvent);
        }

        public static void SetPrimitiveEvent(this IState state, PrimitiveEvent primitiveEvent)
        {
            state.Replace(StateKeys.PrimitiveEvent, primitiveEvent);
        }

        public static PrimitiveEvent GetPrimitiveEvent(this IState state)
        {
            return state.Get<PrimitiveEvent>(StateKeys.PrimitiveEvent);
        }

        public static void SetProjectionEvent(this IState state, ProjectionEvent projectionEvent)
        {
            state.Replace(StateKeys.ProjectionEvent, projectionEvent);
        }

        public static ProjectionEvent GetProjectionEvent(this IState state)
        {
            return state.Get<ProjectionEvent>(StateKeys.ProjectionEvent);
        }

        public static Projection GetProjection(this IState state)
        {
            return state.Get<Projection>(StateKeys.Projection);
        }

        public static void SetProjection(this IState state, Projection projection)
        {
            state.Replace(StateKeys.Projection, projection);
        }

        public static string GetProjectionName(this IState state)
        {
            return state.Get<string>(StateKeys.ProjectionName);
        }

        public static void SetProjectionName(this IState state, string projectionName)
        {
            state.Replace(StateKeys.ProjectionName, projectionName);
        }

        public static Guid GetId(this IState state)
        {
            return state.Get<Guid>(StateKeys.Id);
        }

        public static void SetId(this IState state, Guid id)
        {
            state.Replace(StateKeys.Id, id);
        }

        public static long GetSequenceNumber(this IState state)
        {
            return state.Get<long>(StateKeys.SequenceNumber);
        }

        public static void SetSequenceNumber(this IState state, long sequenceNumber)
        {
            state.Replace(StateKeys.SequenceNumber, sequenceNumber);
        }
    }
}