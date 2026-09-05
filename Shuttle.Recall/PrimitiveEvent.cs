using Shuttle.Contract;

namespace Shuttle.Recall;

public class PrimitiveEvent(Guid id, Guid eventId, int version, string eventType, byte[] eventEnvelope, DateTimeOffset recordedAt, Guid? correlationId = null, long? sequenceNumber = null)
{
    public Guid? CorrelationId { get; private set; } = correlationId;
    public DateTimeOffset RecordedAt { get; } = recordedAt;
    public byte[] EventEnvelope { get; } = eventEnvelope;
    public Guid EventId { get; } = eventId;
    public string EventType { get; } = eventType;
    public Guid Id { get; } = id;
    public long? SequenceNumber { get; private set; } = sequenceNumber;
    public int Version { get; } = version;

    public PrimitiveEvent WithCorrelationId(Guid correlationId)
    {
        CorrelationId = correlationId;
        return this;
    }

    public PrimitiveEvent WithSequenceNumber(long sequenceNumber)
    {
        SequenceNumber = sequenceNumber;
        return this;
    }

    public class Specification
    {
        private readonly List<string> _eventTypes = [];
        private readonly List<Guid> _ids = [];
        private readonly List<int> _versions = [];
        private List<long> _sequenceNumbers = [];

        public IEnumerable<string> EventTypes => _eventTypes.AsReadOnly();
        public bool HasEventTypes => _eventTypes.Any();
        public bool HasIds => _ids.Any();
        public bool HasVersions => _versions.Any();
        public bool HasSequenceNumbers => _sequenceNumbers.Any();

        public IEnumerable<Guid> Ids => _ids.AsReadOnly();
        public IEnumerable<int> Versions => _versions.AsReadOnly();
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
            Guard.AgainstEmpty(eventType);

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
            Guard.AgainstNull(id);

            if (!_ids.Contains(id))
            {
                _ids.Add(id);
            }

            return this;
        }

        public Specification AddIds(IEnumerable<Guid> ids)
        {
            foreach (var id in ids)
            {
                AddId(id);
            }

            return this;
        }

        public Specification AddVersion(int version)
        {
            if (!_versions.Contains(version))
            {
                _versions.Add(version);
            }

            return this;
        }

        public Specification AddVersions(IEnumerable<int> versions)
        {
            foreach (var version in versions)
            {
                AddVersion(version);
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