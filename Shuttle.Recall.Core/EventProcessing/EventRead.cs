using System;

namespace Shuttle.Recall.Core
{
    public class EventRead
    {
        public Event Event { get; private set; }
        public Guid Id { get; private set; }
        public long SequenceNumber { get; private set; }
        public DateTime DateRegistered { get; private set; }
    }
}