namespace Shuttle.Recall
{
    public interface IEventProjectionRepository
    {
        long GetSequenceNumber(string projectionName);
        void SetSequenceNumber(string projectionName, long sequenceNumber);
    }
}