namespace Shuttle.Recall.WebApi.Contracts.v1;

public class EventStoreResponse<T>
{
    public List<T> Items { get; set; } = [];
}
