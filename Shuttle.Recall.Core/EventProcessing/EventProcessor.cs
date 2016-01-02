using System;
using System.Collections.Generic;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Core
{
    public class EventProcessor : IEventProcessor
    {
	    public IEventProcessorConfiguration Configuration { get; private set; }
	    private volatile bool _started;
        private readonly List<ProcessorThread> _processorThreads = new List<ProcessorThread>();

        private readonly List<IEventProjection> _eventProjections = new List<IEventProjection>();

        public EventProcessor(IEventProcessorConfiguration configuration)
        {
            Guard.AgainstNull(configuration, "configuration");

            Configuration = configuration;

	        Events = new EventProcessorEvents();
        }

        public void Dispose()
        {
            Stop();
        }

        public void Start()
        {
            if (Started)
            {
                return;
            }

			foreach (var module in Configuration.Modules)
			{
				module.Initialize(this);
			}

	        Configuration.ProjectionService.AttemptInitialization(this);

			foreach (var eventProjection in _eventProjections)
            {
                var processorThread = new ProcessorThread(string.Format("EventQueue-{0}", eventProjection.Name),
                    new EventProjectionProcessor(this, eventProjection));

                processorThread.Start();

                _processorThreads.Add(processorThread);
            }

            _started = true;
        }

        public void Stop()
        {
            if (!Started)
            {
                return;
            }

            foreach (var processorThread in _processorThreads)
            {
                processorThread.Stop();
            }

            _started = false;
        }

        public bool Started
        {
            get { return _started; }
        }

        public void AddEventProjection(IEventProjection eventProjection)
        {
            Guard.AgainstNull(eventProjection, "eventProjection");

            if (_started)
            {
                throw new EventProcessingException(string.Format(RecallResources.EventProcessorStartedCannotAddQueue,
                    eventProjection.Name));
            }

            if (
                _eventProjections.Find(
                    queue => queue.Name.Equals(eventProjection.Name, StringComparison.InvariantCultureIgnoreCase)) != null)
            {
                throw new EventProcessingException(string.Format(RecallResources.DuplicateEventQueueName, eventProjection.Name));
            }

            _eventProjections.Add(eventProjection);
        }

	    public IEventProcessorEvents Events { get; private set; }

		public static IEventProcessor Create()
		{
			return Create(null);
		}

		public static IEventProcessor Create(Action<DefaultConfigurator> configure)
		{
			var configurator = new DefaultConfigurator();

			if (configure != null)
			{
				configure.Invoke(configurator);
			}

			return new EventProcessor(configurator.Configuration());
		}

	}
}