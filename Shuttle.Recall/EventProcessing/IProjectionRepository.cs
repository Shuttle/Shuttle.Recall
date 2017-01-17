namespace Shuttle.Recall
{
    public interface IProjectionRepository
    {
        long GetSequenceNumber(string projectionName);
        void SetSequenceNumber(string projectionName, long sequenceNumber);
    }
}