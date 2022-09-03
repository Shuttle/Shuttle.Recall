using System;
using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Contract;

namespace Shuttle.Recall
{
    public class PrimitiveEvent
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
        public bool IsSnapshot { get; set; }
        public Guid EventId { get; set; }
        public string EventType { get; set; }
        public byte[] EventEnvelope { get; set; }
        public long SequenceNumber { get; set; }
        public DateTime DateRegistered { get; set; }

        public class Specification
        {
            public long SequenceNumberStart { get; private set; }
            public int Count { get; private set; }
            
            private readonly List<Type> _eventTypes = new List<Type>();
            private readonly List<Guid> _ids = new List<Guid>();

            public Specification WithRange(long sequenceNumberStart, int count)
            {
                SequenceNumberStart = sequenceNumberStart;
                Count = count;

                return this;
            }

            public Specification AddEventType<T>()
            {
                AddEventType(typeof(T));

                return this;
            }

            public Specification AddEventType(Type type)
            {
                Guard.AgainstNull(type, nameof(type));

                if (!_eventTypes.Contains(type))
                {
                    _eventTypes.Add(type);
                }

                return this;
            }

            public IEnumerable<Type> EventTypes => _eventTypes.AsReadOnly();

            public Specification AddEventTypes(IEnumerable<Type> types)
            {
                foreach (var type in types ?? Enumerable.Empty<Type>())
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

            public IEnumerable<Guid> Ids => _ids.AsReadOnly();

            public Specification AddIds(IEnumerable<Guid> ids)
            {
                foreach (var type in ids ?? Enumerable.Empty<Guid>())
                {
                    AddId(type);
                }

                return this;
            }
        }
    }
}