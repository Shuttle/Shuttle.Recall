using System.Threading.Tasks;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public interface IProjectionService
{
    Task<ProjectionEvent?> GetEventAsync(IPipelineContext<OnGetEvent> pipelineContext);
    Task AcknowledgeEventAsync(IPipelineContext<OnAcknowledgeEvent> pipelineContext);
}