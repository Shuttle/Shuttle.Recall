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
            public Guid Id { get; private set; }
            public long SequenceNumberStart { get; private set; }
            public int Count { get; private set; }
            
            private readonly List<Type> _eventTypes = new List<Type>();

            public Specification WithSequenceNumberStart(long sequenceNumberStart)
            {
                SequenceNumberStart = sequenceNumberStart;

                return this;
            }

            public Specification WithCount(int count)
            {
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
        }
    }
}