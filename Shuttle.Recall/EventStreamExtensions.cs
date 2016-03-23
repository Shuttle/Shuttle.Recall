namespace Shuttle.Recall
{
    public static class EventStreamExtensions
    {
        public static void EmptyInvariant(this EventStream stream)
        {
            if (stream == null || stream.IsEmpty)
            {
                throw new EventStreamEmptyException();
            }
        }
    }
}