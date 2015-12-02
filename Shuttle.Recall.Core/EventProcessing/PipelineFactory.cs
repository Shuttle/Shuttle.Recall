using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Core
{
    public class DefaultPipelineFactory : IPipelineFactory
    {
        private readonly ReusableObjectPool<Pipeline> _pool;

        public DefaultPipelineFactory()
        {
            _pool = new ReusableObjectPool<Pipeline>();
        }

        public TPipeline GetPipeline<TPipeline>(IEventProcessorConfiguration configuration) where TPipeline : Pipeline
        {
            Guard.AgainstNull(configuration, "configuration");

            var pipeline = (TPipeline)(_pool.Get(typeof(TPipeline)) ?? Activator.CreateInstance(typeof(TPipeline), configuration));

            pipeline.State.Clear();
            pipeline.State.Add(configuration);

            return pipeline;
        }

        public void ReleasePipeline(Pipeline pipeline)
        {
            Guard.AgainstNull(pipeline, "pipeline");

            _pool.Release(pipeline);
        }
    }
}