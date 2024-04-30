namespace Shuttle.Recall
{
    public class EventStreamBuilder
    {
        public bool ShouldIgnoreConnectionRequest { get; private set; }

        public EventStreamBuilder IgnoreConnectionRequest()
        {
            ShouldIgnoreConnectionRequest = true;

            return this;
        }
    }
}