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

        private readonly List<IEventProjection> _eventProjections = new List<IEventProjection>();

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

            foreach (var eventProjection in _eventProjections)
            {
                var processorThread = new ProcessorThread(string.Format("EventQueue-{0}", eventProjection.Name),
                    new EventProjectionProcessor(_configuration, eventProjection));

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
    }
}