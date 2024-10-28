using System;
using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Contract;

namespace Shuttle.Recall;

public class PrimitiveEvent
{
    public PrimitiveEvent()
    {
    }

    public PrimitiveEvent(string eventType)
    {
        EventType = eventType;
    }

    public DateTime DateRegistered { get; set; }
    public byte[] EventEnvelope { get; set; } = Array.Empty<byte>();
    public Guid EventId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public Guid Id { get; set; }
    public long SequenceNumber { get; set; }
    public int Version { get; set; }

    public class Specification
    {
        private readonly List<Type> _eventTypes = new();
        private readonly List<Guid> _ids = new();
        public int Count { get; private set; }

        public IEnumerable<Type> EventTypes => _eventTypes.AsReadOnly();

        public IEnumerable<Guid> Ids => _ids.AsReadOnly();
        public long SequenceNumberStart { get; private set; }

        public Specification AddEventType<T>()
        {
            AddEventType(typeof(T));

            return this;
        }

        public Specification AddEventType(Type type)
        {
            if (!_eventTypes.Contains(Guard.AgainstNull(type)))
            {
                _eventTypes.Add(type);
            }

            return this;
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
            if (!_ids.Contains(id))
            {
                _ids.Add(id);
            }

            return this;
        }

        public Specification AddIds(IEnumerable<Guid> ids)
        {
            foreach (var type in ids ?? Enumerable.Empty<Guid>())
            {
                AddId(type);
            }

            return this;
        }

        public Specification WithRange(long sequenceNumberStart, int count)
        {
            SequenceNumberStart = sequenceNumberStart;
            Count = count;

            return this;
        }
    }
}