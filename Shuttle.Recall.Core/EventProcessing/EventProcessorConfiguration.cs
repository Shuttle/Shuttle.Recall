using System;
using System.Configuration;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Core
{
    public class EventProcessorConfiguration : IEventProcessorConfiguration
    {
        private static readonly object Padlock = new object();

        public static readonly TimeSpan[] DefaultDurationToSleepWhenIdle =
            (TimeSpan[])
                new StringDurationArrayConverter()
                    .ConvertFrom("250ms*4,500ms*2,1s");

        private TimeSpan[] _durationToSleepWhenIdle;
        private IProjectionPosition _projectionPosition;
        private IProjectionEventReader _projectionEventReader;

        private IPipelineFactory _pipelineFactory;

        public EventProcessorConfiguration(IProjectionPosition projectionPosition, IProjectionEventReader projectionEventReader)
        {
            ProjectionPosition = projectionPosition;
            ProjectionEventReader = projectionEventReader;
        }

        public TimeSpan[] DurationToSleepWhenIdle
        {
            get
            {
                return _durationToSleepWhenIdle ??
                       Synchronised(() => _durationToSleepWhenIdle = DefaultDurationToSleepWhenIdle);
            }
            set { _durationToSleepWhenIdle = value; }
        }

        public IPipelineFactory PipelineFactory
        {
            get { return _pipelineFactory ?? Synchronised(() => _pipelineFactory = new DefaultPipelineFactory()); }
            set { _pipelineFactory = value; }
        }

        public IProjectionPosition ProjectionPosition
        {
            get
            {
                if (_projectionPosition == null)
                {
                    throw new ConfigurationErrorsException(RecallResources.MissingProjectionPositionException);
                }

                return _projectionPosition;
            }
            set { _projectionPosition = value; }
        }

        public IProjectionEventReader ProjectionEventReader
        {
            get
            {
                if (_projectionEventReader == null)
                {
                    throw new ConfigurationErrorsException(RecallResources.MissingEventReaderException);
                }

                return _projectionEventReader;
            }
            set { _projectionEventReader = value; }
        }

        private static T Synchronised<T>(Func<T> f)
        {
            lock (Padlock)
            {
                return f.Invoke();
            }
        }
    }
}