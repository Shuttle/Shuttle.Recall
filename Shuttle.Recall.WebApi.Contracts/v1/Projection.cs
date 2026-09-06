namespace Shuttle.Recall.WebApi.Contracts.v1;

public class Projection
{
    public string Name { get; set; } = string.Empty;
    public long SequenceNumber { get; set; }
    public int FailureCount { get; set; }
    public DateTimeOffset? DeferredUntil { get; set; }

    public class Specification
    {
        public string NameMatch { get; set; } = string.Empty;
        public int FailureCountStart { get; set; }
        public long SequenceNumberStart { get; set; }
        public bool? Deferred { get; set; }
        public int MaximumRows { get; set; }
    }
}
