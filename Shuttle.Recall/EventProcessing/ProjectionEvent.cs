namespace Shuttle.Recall;

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

    public bool HasPrimitiveEvent => PrimitiveEvent != null;
    public PrimitiveEvent? PrimitiveEvent { get; }

    public long SequenceNumber { get; }
}