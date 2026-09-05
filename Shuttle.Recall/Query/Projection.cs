using Shuttle.Contract;

namespace Shuttle.Recall.Query;

public class Projection
{
    public string Name { get; set; } = string.Empty;
    public long SequenceNumber { get; set; }
    public int FailureCount { get; set; }
    public DateTimeOffset? DeferredUntil { get; set; }

    public Recall.Projection ToActual()
    {
        return new(Name, SequenceNumber, FailureCount, DeferredUntil);
    }

    public class Specification
    {
        public string NameMatch { get; private set; } = string.Empty;
        public int? FailureCountStart { get; set; }
        public long? SequenceNumberStart { get; set; }
        public bool? Deferred { get; set; }
 
        public Specification WithNameMatch(string nameMatch)
        {
            NameMatch = Guard.AgainstEmpty(nameMatch);
            return this;
        }

        public Specification WithFailureCountStart(int failureCountStart)
        {
            FailureCountStart = failureCountStart;
            return this;
        }

        public Specification WithSequenceNumberStart(long sequenceNumberStart)
        {
            SequenceNumberStart = sequenceNumberStart;
            return this;
        }

        public Specification WithDeferred(bool deferred)
        {
            Deferred = deferred;
            return this;
        }
    }
}