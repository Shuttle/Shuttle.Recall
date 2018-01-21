namespace Shuttle.Recall
{
    public class DomainEvent
    {
        public DomainEvent(object @event, int version)
        {
            Event = @event;
            Version = version;
            IsSnapshot = false;
        }

        public object Event { get; }
        public bool IsSnapshot { get; private set; }
        public int Version { get; }

        public DomainEvent AsSnapshot()
        {
            IsSnapshot = true;

            return this;
        }
    }
}