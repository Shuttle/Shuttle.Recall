using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class ProcessEventObserver : IPipelineObserver<OnProcessEvent>
    {
        private readonly ILog _log;

        public ProcessEventObserver()
        {
            _log = Log.For(this);
        }

        public void Execute(OnProcessEvent pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var primitiveEvent = state.GetPrimitiveEvent();
            var eventEnvelope = state.GetEventEnvelope();
            var projection = state.GetProjection();
            var domainEvent = state.GetEvent();

            Guard.AgainstNull(primitiveEvent, "state.GetPrimitiveEvent()");
            Guard.AgainstNull(eventEnvelope, "state.GetEventEnvelope()");
            Guard.AgainstNull(projection, "state.GetProjection()");
            Guard.AgainstNull(domainEvent, "state.GetEvent()");

            var type = Type.GetType(eventEnvelope.AssemblyQualifiedName);

            if (!projection.HandlesType(type))
            {
                if (Log.IsTraceEnabled)
                {
                    _log.Trace(string.Format(RecallResources.TraceTypeNotHandled, projection.Name, eventEnvelope.AssemblyQualifiedName));
                }

                return;
            }

            projection.Process(eventEnvelope, domainEvent, primitiveEvent, state.GetThreadState());
        }
    }
}