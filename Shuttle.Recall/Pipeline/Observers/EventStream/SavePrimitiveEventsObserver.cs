using System;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Serialization;
using Shuttle.Core.Streams;

namespace Shuttle.Recall
{
    public interface ISavePrimitiveEventsObserver : IPipelineObserver<OnSavePrimitiveEvents>
    {
    }

    public class SavePrimitiveEventsObserver : ISavePrimitiveEventsObserver
    {
        private readonly IConcurrencyExceptionSpecification _concurrencyExceptionSpecification;
        private readonly IPrimitiveEventRepository _primitiveEventRepository;
        private readonly ISerializer _serializer;

        public SavePrimitiveEventsObserver(IPrimitiveEventRepository primitiveEventRepository, ISerializer serializer, IConcurrencyExceptionSpecification concurrencyExceptionSpecification)
        {
            _primitiveEventRepository = Guard.AgainstNull(primitiveEventRepository, nameof(primitiveEventRepository));
            _serializer = Guard.AgainstNull(serializer, nameof(serializer));
            _concurrencyExceptionSpecification = Guard.AgainstNull(concurrencyExceptionSpecification, nameof(concurrencyExceptionSpecification));
        }

        public void Execute(OnSavePrimitiveEvents pipelineEvent)
        {
            ExecuteAsync(pipelineEvent, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnSavePrimitiveEvents pipelineEvent)
        {
            await ExecuteAsync(pipelineEvent, false).ConfigureAwait(false);
        }

        private async Task ExecuteAsync(OnSavePrimitiveEvents pipelineEvent, bool sync)
        {
            var state = Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent)).Pipeline.State;
            var eventStream = Guard.AgainstNull(state.GetEventStream(), StateKeys.EventStream);
            var eventEnvelopes = Guard.AgainstNull(state.GetEventEnvelopes(), StateKeys.EventEnvelopes);

            var version = -1;
            long sequenceNumber = 0;

            try
            {
                foreach (var eventEnvelope in eventEnvelopes)
                {
                    version = eventEnvelope.Version;

                    var primitiveEvent = new PrimitiveEvent
                    {
                        Id = eventStream.Id,
                        EventEnvelope = _serializer.Serialize(eventEnvelope).ToBytes(),
                        EventId = eventEnvelope.EventId,
                        EventType = eventEnvelope.EventType,
                        IsSnapshot = eventEnvelope.IsSnapshot,
                        Version = version,
                        DateRegistered = eventEnvelope.EventDate
                    };

                    if (sync)
                    {
                        sequenceNumber = _primitiveEventRepository.Save(primitiveEvent);
                    }
                    else
                    {
                        sequenceNumber = await _primitiveEventRepository.SaveAsync(primitiveEvent).ConfigureAwait(false);
                    }

                    if (sequenceNumber > 0)
                    {
                        state.SetSequenceNumber(sequenceNumber);
                    }
                }

                if (sequenceNumber < 1)
                {
                    state.SetSequenceNumber(sync ? _primitiveEventRepository.GetSequenceNumber(eventStream.Id) : await _primitiveEventRepository.GetSequenceNumberAsync(eventStream.Id).ConfigureAwait(false));
                }
            }
            catch (Exception ex)
            {
                if (_concurrencyExceptionSpecification.IsSatisfiedBy(ex))
                {
                    throw new EventStreamConcurrencyException(string.Format(Resources.EventStreamConcurrencyException, eventStream.Id, version), ex);
                }

                throw;
            }
        }
    }
}