using System;

namespace Shuttle.Recall.Core
{
    public interface IEventProcessorConfiguration
    {
        TimeSpan[] DurationToSleepWhenIdle { get; set; }
        IPipelineFactory PipelineFactory { get; set; }
        IProjectionPosition ProjectionPosition { get; set; }
        IProjectionEventReader ProjectionEventReader { get; set; }
    }
}