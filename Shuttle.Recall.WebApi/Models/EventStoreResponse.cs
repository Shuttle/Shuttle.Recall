namespace Shuttle.Recall.WebApi.Models;

public class EventStoreResponse<T>
{
    public bool Authorized { get; set; }
    public List<T> Items { get; set; } = [];
}