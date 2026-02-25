namespace Shuttle.Recall.WebApi.Models;

public class EventStoreResponse<T>
{
    public List<T> Items { get; set; } = [];
}