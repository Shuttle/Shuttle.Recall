using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Core
{
    public class ProcessEventObserver : IPipelineObserver<OnProcessEvent>
    {
        public void Execute(OnProcessEvent pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var domainEvent = state.GetDomainEvent();
            var projector = state.Get<IEventProjector>();

            if (!projector.HandlesType(domainEvent.GetType()))
            {
                return;
            }

            projector.Process(domainEvent);
        }
    }
}