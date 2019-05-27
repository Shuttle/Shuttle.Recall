namespace Shuttle.Recall
{
    public class ProjectionEvent
    {
        public ProjectionEvent(long sequenceNumber)
        {
            SequenceNumber = sequenceNumber;
        }

        public ProjectionEvent(PrimitiveEvent primitiveEvent)
        {
            SequenceNumber = primitiveEvent.SequenceNumber;
            PrimitiveEvent = primitiveEvent;
        }

        public long SequenceNumber { get; }
        public PrimitiveEvent PrimitiveEvent { get; }
        public bool HasPrimitiveEvent => PrimitiveEvent != null;
    }
}