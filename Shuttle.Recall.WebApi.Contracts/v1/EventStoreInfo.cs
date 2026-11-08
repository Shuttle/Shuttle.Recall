namespace Shuttle.Recall.WebApi.Contracts.v1;

public class EventStoreInfo
{
    public string Name { get; set; } = string.Empty;
    public bool HasAccess { get; set; }
    public List<string> ProjectionTables { get; set; } = [];
}
