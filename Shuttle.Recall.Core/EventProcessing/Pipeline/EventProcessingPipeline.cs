using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Core
{
    public class EventProcessingPipeline : Pipeline
    {
        public EventProcessingPipeline()
        {
            RegisterStage("Process")
                .WithEvent<OnGetEvent>()
                .WithEvent<OnAfterGetEvent>()
                .WithEvent<OnProcessEvent>()
                .WithEvent<OnAfterProcessEvent>()
                .WithEvent<OnAcknowledgeEvent>()
                .WithEvent<OnAfterAcknowledgeEvent>();

            RegisterObserver(new GetEventObserver());
            RegisterObserver(new ProcessEventObserver());
            RegisterObserver(new AcknowledgeEventObserver());
        }
    }
}