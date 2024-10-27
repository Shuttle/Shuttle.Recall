using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public class AddProjectionPipeline : Pipeline
{
    public AddProjectionPipeline(IAddProjectionObserver addProjectionObserver)
    {
        RegisterStage("AddProjection")
            .WithEvent<OnBeforeAddProjection>()
            .WithEvent<OnAddProjection>()
            .WithEvent<OnAfterAddProjection>();

        RegisterObserver(Guard.AgainstNull(addProjectionObserver));
    }

    public async Task ExecuteAsync(string name)
    {
        State.SetProjectionName(name);

        await ExecuteAsync();
    }
}