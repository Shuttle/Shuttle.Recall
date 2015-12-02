namespace Shuttle.Recall.Core
{
    public interface IEventProjectorPosition
    {
        long GetSequenceNumber(string name);
        void SetSequenceNumber(string name, long sequenceNumber);
    }
}