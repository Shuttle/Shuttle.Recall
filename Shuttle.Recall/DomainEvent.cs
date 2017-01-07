namespace Shuttle.Recall
{
    public class DomainEvent
    {
        public object Event { get; private set; }
        public bool IsSnapshot { get; private set; }
        public int Version { get; private set; }

        public DomainEvent(object @event, int version)
        {
            Event = @event;
            Version = version;
            IsSnapshot = false;
        }

        public DomainEvent AsSnapshot()
        {
            IsSnapshot = true;

            return this;
        }
    }
}