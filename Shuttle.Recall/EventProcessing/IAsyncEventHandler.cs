using System.Threading.Tasks;

namespace Shuttle.Recall;

public interface IAsyncEventHandler<in T> where T : class
{
    Task ProcessEventAsync(IEventHandlerContext<T> context);
}