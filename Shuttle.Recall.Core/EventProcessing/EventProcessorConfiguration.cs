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
        private IEventProjectorPosition _eventProjectorPosition;
        private IEventReader _eventReader;

        private IPipelineFactory _pipelineFactory;

        public EventProcessorConfiguration(IEventProjectorPosition eventProjectorPosition, IEventReader eventReader)
        {
            EventProjectorPosition = eventProjectorPosition;
            EventReader = eventReader;
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

        public IEventProjectorPosition EventProjectorPosition
        {
            get
            {
                if (_eventProjectorPosition == null)
                {
                    throw new ConfigurationErrorsException(RecallResources.MissingEventProjectorPositionException);
                }

                return _eventProjectorPosition;
            }
            set { _eventProjectorPosition = value; }
        }

        public IEventReader EventReader
        {
            get
            {
                if (_eventReader == null)
                {
                    throw new ConfigurationErrorsException(RecallResources.MissingEventReaderException);
                }

                return _eventReader;
            }
            set { _eventReader = value; }
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