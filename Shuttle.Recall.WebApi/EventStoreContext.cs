using Microsoft.Extensions.Options;
using Shuttle.Contract;

namespace Shuttle.Recall.WebApi;

public class EventStoreContext : IEventStoreContext
{
    public const string EventStoreHeaderName = "Shuttle-Recall-Event-Store";

    public EventStoreContext(IHttpContextAccessor httpContextAccessor, IOptions<ApiOptions> apiOptions)
    {
        var eventStores = Guard.AgainstNull(Guard.AgainstNull(apiOptions).Value).EventStores;

        if (eventStores.Count == 0)
        {
            throw new ApplicationException("No event stores have been configured under 'Shuttle:Recall:Api:EventStores'.");
        }

        var name = Guard.AgainstNull(httpContextAccessor).HttpContext?.Request.Headers[EventStoreHeaderName].FirstOrDefault();

        EventStore = (string.IsNullOrWhiteSpace(name)
            ? null
            : eventStores.FirstOrDefault(item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase))) ?? eventStores[0];
    }

    public ApiOptions.EventStoreOptions EventStore { get; }
}
