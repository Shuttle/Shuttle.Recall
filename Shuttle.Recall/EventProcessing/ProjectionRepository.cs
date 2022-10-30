namespace Shuttle.Recall
{
    public class NotImplementedProjectionRepository : IProjectionRepository
    {
        public Projection Find(string name)
        {
            throw new System.NotImplementedException();
        }

        public void Save(Projection projection)
        {
            throw new System.NotImplementedException();
        }

        public void SetSequenceNumber(string projectionName, long sequenceNumber)
        {
            throw new System.NotImplementedException();
        }

        public long GetSequenceNumber(string projectionName)
        {
            throw new System.NotImplementedException();
        }
    }
}