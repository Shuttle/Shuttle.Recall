using System;

namespace Shuttle.Recall.Core
{
    public interface IEventProcessorConfiguration
    {
        TimeSpan[] DurationToSleepWhenIdle { get; set; }
        IPipelineFactory PipelineFactory { get; set; }
        IEventProjectorPosition EventProjectorPosition { get; set; }
        IEventReader EventReader { get; set; }
    }
}