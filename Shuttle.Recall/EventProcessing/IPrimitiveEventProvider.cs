namespace Shuttle.Recall
{
    public interface IPrimitiveEventProvider
    {
        bool IsEmpty { get; }

        void Completed(long sequenceNumber);

        ProjectionEvent Get(Projection projection);
    }
}