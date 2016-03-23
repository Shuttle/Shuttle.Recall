using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public interface IPipelineFactory
    {
        TPipeline GetPipeline<TPipeline>(IEventProcessor eventProcessor) where TPipeline : Pipeline;
        void ReleasePipeline(Pipeline pipeline);
    }
}