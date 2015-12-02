using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Core
{
    public interface IPipelineFactory
    {
        TPipeline GetPipeline<TPipeline>(IEventProcessorConfiguration configuration) where TPipeline : Pipeline;
        void ReleasePipeline(Pipeline pipeline);
    }
}