using System;
using System.Configuration;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class EventProcessorConfiguration : IEventProcessorConfiguration
    {
        private static readonly object Padlock = new object();
	    private static EventProcessorSection _section;

        public static readonly TimeSpan[] DefaultDurationToSleepWhenIdle =
            (TimeSpan[])
                new StringDurationArrayConverter()
                    .ConvertFrom("250ms*4,500ms*2,1s");

        private TimeSpan[] _durationToSleepWhenIdle;
        private IProjectionService _projectionService;

		public ModuleCollection Modules { get; private set; }

		private IPipelineFactory _pipelineFactory;

        public EventProcessorConfiguration()
        {
			Modules = new ModuleCollection();
		}

		public static EventProcessorSection Section
		{
			get { return _section ?? Synchronised(() => _section = ConfigurationSectionProvider.Open<EventProcessorSection>("shuttle", "eventProcessor")); }
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

        public IProjectionService ProjectionService
        {
            get
            {
                if (_projectionService == null)
                {
                    throw new ConfigurationErrorsException(RecallResources.MissingProjectionServiceException);
                }

                return _projectionService;
            }
            set { _projectionService = value; }
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