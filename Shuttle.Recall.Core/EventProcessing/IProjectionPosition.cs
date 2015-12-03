namespace Shuttle.Recall.Core
{
    public interface IProjectionPosition
    {
        long GetSequenceNumber(string name);
        void SetSequenceNumber(string name, long sequenceNumber);
    }
}