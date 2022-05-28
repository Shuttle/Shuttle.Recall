namespace Shuttle.Recall.Tests
{
    public class EventHandler :
        IEventHandler<EventA>,
        IEventHandler<EventB>
    {
        private static readonly object Lock = new();
        public int Entry { get; private set; }

        public void ProcessEvent(IEventHandlerContext<EventA> context)
        {
            Apply(context.Event.Entry);
        }

        public void ProcessEvent(IEventHandlerContext<EventB> context)
        {
            Apply(context.Event.Entry);
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
}