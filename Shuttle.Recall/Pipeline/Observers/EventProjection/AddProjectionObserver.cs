using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall
{
    public interface IAddProjectionObserver : IPipelineObserver<OnAddProjection>
    {
    }

    public class AddProjectionObserver : IAddProjectionObserver
    {
        private readonly EventStoreOptions _eventStoreOptions;
        private readonly IProjectionRepository _repository;

        public AddProjectionObserver(IOptions<EventStoreOptions> eventStoreOptions, IProjectionRepository projectionRepository)
        {
            _eventStoreOptions = Guard.AgainstNull(eventStoreOptions, nameof(eventStoreOptions)).Value;
            _repository = Guard.AgainstNull(projectionRepository, nameof(projectionRepository));    
        }

        public void Execute(OnAddProjection pipelineEvent)
        {
            ExecuteAsync(pipelineEvent, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnAddProjection pipelineEvent)
        {
            await ExecuteAsync(pipelineEvent, false).ConfigureAwait(false);
        }

        private async Task ExecuteAsync(OnAddProjection pipelineEvent, bool sync)
        {
            var state = Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent)).Pipeline.State;
            var name = Guard.AgainstNullOrEmptyString(state.GetProjectionName(), StateKeys.ProjectionName);

            var projection = sync
                ? _repository.Find(name)
                : await _repository.FindAsync(name).ConfigureAwait(false);

            if (projection == null)
            {
                projection = new Projection(_eventStoreOptions, name, 0);

                if (sync)
                {
                    _repository.Save(projection);
                }
                else
                {
                    await _repository.SaveAsync(projection).ConfigureAwait(false);
                }
            }

            state.SetProjection(projection);
        }
    }
}