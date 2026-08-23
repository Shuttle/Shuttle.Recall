namespace Shuttle.Recall.WebApi;

public interface IEventStoreContext
{
    ApiOptions.EventStoreOptions EventStore { get; }
}
