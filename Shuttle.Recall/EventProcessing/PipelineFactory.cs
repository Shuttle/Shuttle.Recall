using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class DefaultPipelineFactory : IPipelineFactory
    {
        private readonly ReusableObjectPool<Pipeline> _pool;

        public DefaultPipelineFactory()
        {
            _pool = new ReusableObjectPool<Pipeline>();
        }

        public TPipeline GetPipeline<TPipeline>(IEventProcessor eventProcessor) where TPipeline : Pipeline
        {
            Guard.AgainstNull(eventProcessor, "eventProcessor");

            var pipeline = (TPipeline)(_pool.Get(typeof(TPipeline)) ?? Activator.CreateInstance(typeof(TPipeline)));

            pipeline.State.Clear();
            pipeline.State.Add(eventProcessor);
            pipeline.State.Add(eventProcessor.Configuration);

			eventProcessor.Events.OnPipelineCreated(this, new PipelineEventArgs(pipeline));

			return pipeline;
        }

        public void ReleasePipeline(Pipeline pipeline)
        {
            Guard.AgainstNull(pipeline, "pipeline");

            _pool.Release(pipeline);
        }
    }
}