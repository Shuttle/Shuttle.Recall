namespace Shuttle.Recall.WebApi.Contracts.v1;

public class EventType
{
    public Guid Id { get; set; }
    public string TypeName { get; set; } = string.Empty;

    public class Specification
    {
        public int MaximumRows { get; set; }
        public string TypeNameMatch { get; set; } = string.Empty;
    }
}
