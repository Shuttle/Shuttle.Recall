using System;
using System.Collections.Generic;
using System.IO;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public static class PipelineStateExtensions
    {
        public static IThreadState GetThreadState(this IState state)
        {
            return state.Get<IThreadState>(StateKeys.ThreadState);
        }

        public static void SetThreadState(this IState state, IThreadState activeState)
        {
            state.Replace(StateKeys.ThreadState, activeState);
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

        public static void SetEventEnvelopeStream(this IState state, Stream value)
        {
            state.Replace(StateKeys.EventEnvelopeStream, value);
        }

        public static Stream GetEventEnvelopeStream(this IState state)
        {
            return state.Get<Stream>(StateKeys.EventEnvelopeStream);
        }

        public static byte[] GetMessageBytes(this IState state)
        {
            return state.Get<byte[]>(StateKeys.MessageBytes);
        }

        public static void SetEventBytes(this IState state, byte[] bytes)
        {
            state.Replace(StateKeys.MessageBytes, bytes);
        }

        public static object GetEvent(this IState state)
        {
            return state.Get<object>(StateKeys.Event);
        }

        public static void SetEvent(this IState state, object @event)
        {
            state.Replace(StateKeys.Event, @event);
        }

        public static void SetPrimitiveEvent(this IState state, PrimitiveEvent primitiveEvent)
        {
            state.Replace(StateKeys.PrimitiveEvent, primitiveEvent);
        }

        public static PrimitiveEvent GetPrimitiveEvent(this IState state)
        {
            return state.Get<PrimitiveEvent>(StateKeys.PrimitiveEvent);
        }

        public static EventProjection GetEventProjection(this IState state)
        {
            return state.Get<EventProjection>(StateKeys.EventProjection);
        }

        public static void SetEventProjection(this IState state, EventProjection eventProjection)
        {
            state.Replace(StateKeys.EventProjection, eventProjection);
        }

        public static Guid GetId(this IState state)
        {
            return state.Get<Guid>(StateKeys.Id);
        }

        public static void SetId(this IState state, Guid id)
        {
            state.Replace(StateKeys.Id, id);
        }
    }
}