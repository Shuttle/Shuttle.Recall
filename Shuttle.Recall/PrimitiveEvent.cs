using Shuttle.Core.Contract;

namespace Shuttle.Recall;

public class PrimitiveEvent
{
    public Guid? CorrelationId { get; set; }
    public DateTime DateRegistered { get; set; }
    public byte[] EventEnvelope { get; set; } = [];
    public Guid EventId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public Guid Id { get; set; }
    public long? SequenceNumber { get; set; }
    public int Version { get; set; }

    public class Specification
    {
        private readonly List<string> _eventTypes = [];
        private readonly List<Guid> _ids = [];
        private List<long> _sequenceNumbers = [];

        public IEnumerable<string> EventTypes => _eventTypes.AsReadOnly();
        public bool HasEventTypes => _eventTypes.Any();
        public bool HasIds => _ids.Any();
        public bool HasSequenceNumbers => _sequenceNumbers.Any();

        public IEnumerable<Guid> Ids => _ids.AsReadOnly();
        public int MaximumRows { get; private set; }
        public long SequenceNumberEnd { get; private set; }
        public IEnumerable<long> SequenceNumbers => _sequenceNumbers;
        public long SequenceNumberStart { get; private set; }

        public Specification AddEventType<T>()
        {
            AddEventType(typeof(T));

            return this;
        }

        public Specification AddEventType(string eventType)
        {
            Guard.AgainstEmpty(eventType, nameof(eventType));

            if (!_eventTypes.Contains(eventType))
            {
                _eventTypes.Add(eventType);
            }

            return this;
        }

        public Specification AddEventType(Type type)
        {
            return AddEventType(Guard.AgainstEmpty(type.FullName));
        }

        public Specification AddEventTypes(IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                AddEventType(type);
            }

            return this;
        }

        public Specification AddId(Guid id)
        {
            Guard.AgainstNull(id, nameof(id));

            if (!_ids.Contains(id))
            {
                _ids.Add(id);
            }

            return this;
        }

        public Specification AddIds(IEnumerable<Guid> ids)
        {
            foreach (var type in ids)
            {
                AddId(type);
            }

            return this;
        }

        public Specification AddSequenceNumber(long sequenceNumber)
        {
            if (!_sequenceNumbers.Contains(sequenceNumber))
            {
                _sequenceNumbers.Add(sequenceNumber);
            }

            return this;
        }

        public Specification AddSequenceNumbers(IEnumerable<long> sequenceNumbers)
        {
            foreach (var sequenceNumber in sequenceNumbers)
            {
                AddSequenceNumber(sequenceNumber);
            }

            return this;
        }

        public Specification WithMaximumRows(int maximumRows)
        {
            MaximumRows = maximumRows;

            return this;
        }

        public Specification WithSequenceNumberEnd(long sequenceNumberEnd)
        {
            SequenceNumberEnd = sequenceNumberEnd;

            return this;
        }

        public Specification WithSequenceNumbers(IEnumerable<long> sequenceNumbers)
        {
            _sequenceNumbers = [.. sequenceNumbers];

            return this;
        }

        public Specification WithSequenceNumberStart(long sequenceNumberStart)
        {
            SequenceNumberStart = sequenceNumberStart;

            return this;
        }
    }
}