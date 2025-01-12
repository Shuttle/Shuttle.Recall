namespace Shuttle.Recall.WebApi.Models;

public class PrimitiveEvent
{
    public class Specification
    {
        public int MaximumRows { get; set; }
        public long SequenceNumberStart { get; set; }
        public List<string> EventTypes { get; set; } = [];
    }
}