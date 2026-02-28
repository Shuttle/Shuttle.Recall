namespace Shuttle.Recall.WebApi.Models;

public class PrimitiveEvent
{
    public class Specification
    {
        public Guid? Id { get; set; }
        public List<string> EventTypes { get; set; } = [];
        public int MaximumRows { get; set; }
        public long SequenceNumberStart { get; set; }
    }
}