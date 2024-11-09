using System.Threading.Tasks;

namespace Shuttle.Recall;

public interface IProjectionEventProvider
{
    Task<ProjectionEvent?> GetAsync();
}