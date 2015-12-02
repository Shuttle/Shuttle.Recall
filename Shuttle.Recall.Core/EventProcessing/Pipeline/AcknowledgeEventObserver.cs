using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Core
{
    public class AcknowledgeEventObserver : IPipelineObserver<OnAcknowledgeEvent>
    {
        public void Execute(OnAcknowledgeEvent pipelineEvent)
        {
            throw new System.NotImplementedException();
        }
    }
}