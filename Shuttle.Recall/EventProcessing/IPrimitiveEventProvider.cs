namespace Shuttle.Recall
{
    public interface IPrimitiveEventProvider
    {
        bool IsEmpty { get; }

        void Completed(long sequenceNumber);

        PrimitiveEvent Get(Projection projection);
    }
}