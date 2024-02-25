using System.Threading.Tasks;

namespace Shuttle.Recall
{
    public interface IProjectionEventProvider
    {
        ProjectionEvent Get(Projection projection);
        Task<ProjectionEvent> GetAsync(Projection projection);
    }
}