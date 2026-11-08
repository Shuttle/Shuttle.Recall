using Shuttle.Recall.WebApi.Contracts.v1;

namespace Shuttle.Recall.WebApi;

public interface IEventStoreRebuilder
{
    Task<Rebuild.Result> RebuildAsync(ApiOptions.EventStoreOptions eventStore, Rebuild rebuild, CancellationToken cancellationToken = default);
}
