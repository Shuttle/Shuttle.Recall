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

            var type = Type.GetType(eventEnvelope.AssemblyQualifiedName);

            if (!projection.HandlesType(type))
            {
                if (Log.IsTraceEnabled)
                {
                    _log.Trace(string.Format(RecallResources.TraceTypeNotHandled, projection.Name, eventEnvelope.AssemblyQualifiedName));
                }

                return;
            }

            projection.Process(eventEnvelope, primitiveEvent.SequenceNumber, state.GetThreadState());
        }
    }
}