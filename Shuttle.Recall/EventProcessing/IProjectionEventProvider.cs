namespace Shuttle.Recall
{
    public interface IProjectionEventProvider
    {
        bool IsEmpty { get; }

        void Completed(long sequenceNumber);

        ProjectionEvent Get(Projection projection);
    }
}