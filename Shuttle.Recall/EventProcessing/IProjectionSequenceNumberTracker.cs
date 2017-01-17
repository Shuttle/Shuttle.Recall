namespace Shuttle.Recall
{
    public interface IProjectionSequenceNumberTracker
    {
        bool Contains(string projectionName);
        long? TryGet(string projectionName);
        void Set(string projectionName, long sequenceNumber);
    }
}