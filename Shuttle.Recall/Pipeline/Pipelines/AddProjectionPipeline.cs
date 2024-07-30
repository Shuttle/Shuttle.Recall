using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall
{
    public class AddProjectionPipeline : Pipeline
    {
        public AddProjectionPipeline(IAddProjectionObserver addProjectionObserver)
        {
            RegisterStage("AddProjection")
                .WithEvent<OnBeforeAddProjection>()
                .WithEvent<OnAddProjection>()
                .WithEvent<OnAfterAddProjection>();

            RegisterObserver(Guard.AgainstNull(addProjectionObserver, nameof(addProjectionObserver)));
        }

        public void Execute(string name)
        {
            ExecuteAsync(name, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(string name)
        {
            await ExecuteAsync(name, false).ConfigureAwait(false);
        }

        private async Task ExecuteAsync(string name, bool sync)
        {
            State.SetProjectionName(name);

            if (sync)
            {
                Execute();
            }
            else
            {
                await ExecuteAsync();
            }
        }
    }
}