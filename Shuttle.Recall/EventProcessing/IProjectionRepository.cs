namespace Shuttle.Recall
{
    public interface IProjectionRepository
    {
        Projection Find(string name);
        void Save(Projection projection);
        void SetSequenceNumber(string projectionName, long sequenceNumber);
    }
}