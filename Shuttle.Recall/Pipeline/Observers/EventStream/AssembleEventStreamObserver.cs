using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class AssembleEventStreamObserver : IPipelineObserver<OnAssembleEventStream>
    {
        private readonly IEventMethodInvoker _eventMethodInvoker;

        public AssembleEventStreamObserver(IEventMethodInvoker eventMethodInvoker)
        {
            Guard.AgainstNull(eventMethodInvoker, "eventMethodInvoker");

            _eventMethodInvoker = eventMethodInvoker;
        }

        public void Execute(OnAssembleEventStream pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;

            state.SetEventStream(new EventStream(state.GetId(), state.GetVersion(), state.GetEvents(), _eventMethodInvoker));
        }
    }
}