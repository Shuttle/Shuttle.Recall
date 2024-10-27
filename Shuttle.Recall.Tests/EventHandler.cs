using System.Threading.Tasks;

namespace Shuttle.Recall.Tests;

public class EventHandler :
    IEventHandler<EventA>,
    IEventHandler<EventB>
{
    private static readonly object Lock = new();
    public int Entry { get; private set; }

    public async Task ProcessEventAsync(IEventHandlerContext<EventA> context)
    {
        Apply(context.Event.Entry);

        await Task.CompletedTask;
    }

    public async Task ProcessEventAsync(IEventHandlerContext<EventB> context)
    {
        Apply(context.Event.Entry);

        await Task.CompletedTask;
    }

    private void Apply(int entry)
    {
        lock (Lock)
        {
            if (entry <= Entry)
            {
                return;
            }

            Entry = entry;
        }
    }
}

public class EventA
{
    public int Entry { get; set; }
}

public class EventB
{
    public int Entry { get; set; }
}