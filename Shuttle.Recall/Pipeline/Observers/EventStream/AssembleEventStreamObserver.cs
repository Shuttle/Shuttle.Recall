using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class AssembleEventStreamObserver : IPipelineObserver<OnAssembleEventStream>
    {
        public void Execute(OnAssembleEventStream pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;

            state.SetEventStream(new EventStream(state.GetId(), state.GetVersion(), state.GetEvents()));
        }
    }
}