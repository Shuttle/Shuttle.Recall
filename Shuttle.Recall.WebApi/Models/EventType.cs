namespace Shuttle.Recall.WebApi.Models;

public class EventType
{
    public class Specification
    {
        public int MaximumRows { get; set; }
        public string TypeNameMatch { get; set; } = string.Empty;
    }
}