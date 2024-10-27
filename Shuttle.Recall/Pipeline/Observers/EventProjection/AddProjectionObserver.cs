using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public interface IAddProjectionObserver : IPipelineObserver<OnAddProjection>
{
}

public class AddProjectionObserver : IAddProjectionObserver
{
    private readonly EventStoreOptions _eventStoreOptions;
    private readonly IProjectionRepository _repository;

    public AddProjectionObserver(IOptions<EventStoreOptions> eventStoreOptions, IProjectionRepository projectionRepository)
    {
        _eventStoreOptions = Guard.AgainstNull(Guard.AgainstNull(eventStoreOptions).Value);
        _repository = Guard.AgainstNull(projectionRepository);
    }

    public async Task ExecuteAsync(IPipelineContext<OnAddProjection> pipelineContext)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var name = Guard.AgainstNullOrEmptyString(state.GetProjectionName());

        var projection = await _repository.FindAsync(name).ConfigureAwait(false);

        if (projection == null)
        {
            projection = new(name, 0);

            await _repository.SaveAsync(projection).ConfigureAwait(false);
        }

        state.SetProjection(projection);
    }
}