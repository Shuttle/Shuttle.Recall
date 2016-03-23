using System;

namespace Shuttle.Recall
{
    public interface IEventProcessorConfiguration
    {
        TimeSpan[] DurationToSleepWhenIdle { get; set; }
        IPipelineFactory PipelineFactory { get; set; }
        IProjectionService ProjectionService { get; set; }
	    ModuleCollection Modules { get; }
    }
}