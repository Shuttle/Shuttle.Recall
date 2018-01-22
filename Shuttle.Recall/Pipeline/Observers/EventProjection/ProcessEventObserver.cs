using System;
using Shuttle.Core.Contract;
using Shuttle.Core.Logging;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall
{
    public interface IProcessEventObserver : IPipelineObserver<OnProcessEvent>
    {
    }

    public class ProcessEventObserver : IProcessEventObserver
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

            Guard.AgainstNull(primitiveEvent, nameof(primitiveEvent));
            Guard.AgainstNull(eventEnvelope, nameof(eventEnvelope));
            Guard.AgainstNull(projection, nameof(projection));
            Guard.AgainstNull(domainEvent, nameof(domainEvent));

            var type = Type.GetType(eventEnvelope.AssemblyQualifiedName);

            if (!projection.HandlesType(type))
            {
                if (Log.IsTraceEnabled)
                {
                    _log.Trace(string.Format(Resources.TraceTypeNotHandled, projection.Name,
                        eventEnvelope.AssemblyQualifiedName));
                }

                return;
            }

            projection.Process(eventEnvelope, domainEvent, primitiveEvent, state.GetThreadState());
        }
    }
}