using System;
using System.Collections.Generic;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Core
{
    public class EventProcessor : IEventProcessor
    {
        private readonly IEventProcessorConfiguration _configuration;
        private volatile bool _started;
        private readonly List<ProcessorThread> _processorThreads = new List<ProcessorThread>();

        private readonly List<IEventProjector> _eventProjectors = new List<IEventProjector>();

        public EventProcessor(IEventProcessorConfiguration configuration)
        {
            Guard.AgainstNull(configuration, "configuration");

            _configuration = configuration;
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

            foreach (var eventProjector in _eventProjectors)
            {
                var processorThread = new ProcessorThread(string.Format("EventQueue-{0}", eventProjector.Name),
                    new EventProjectorProcessor(_configuration, eventProjector));

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

        public void AddEventProjector(IEventProjector eventProjector)
        {
            Guard.AgainstNull(eventProjector, "eventProjector");

            if (_started)
            {
                throw new EventProcessingException(string.Format(RecallResources.EventProcessorStartedCannotAddQueue,
                    eventProjector.Name));
            }

            if (
                _eventProjectors.Find(
                    queue => queue.Name.Equals(eventProjector.Name, StringComparison.InvariantCultureIgnoreCase)) != null)
            {
                throw new EventProcessingException(string.Format(RecallResources.DuplicateEventQueueName, eventProjector.Name));
            }

            _eventProjectors.Add(eventProjector);
        }
    }
}