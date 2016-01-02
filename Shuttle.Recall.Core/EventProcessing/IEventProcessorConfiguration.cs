using System;

namespace Shuttle.Recall.Core
{
    public interface IEventProcessorConfiguration
    {
        TimeSpan[] DurationToSleepWhenIdle { get; set; }
        IPipelineFactory PipelineFactory { get; set; }
        IProjectionService ProjectionService { get; set; }
	    ModuleCollection Modules { get; }
    }
}