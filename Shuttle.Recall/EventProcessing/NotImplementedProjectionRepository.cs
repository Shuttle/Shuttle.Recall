using System;
using System.Threading.Tasks;

namespace Shuttle.Recall
{
    public class NotImplementedProjectionRepository : IProjectionRepository
    {
        public Projection Find(string name)
        {
            throw new NotImplementedException(Resources.NotImplementedProjectionRepository);
        }

        public Task<Projection> FindAsync(string name)
        {
            throw new NotImplementedException(Resources.NotImplementedProjectionRepository);
        }

        public void Save(Projection projection)
        {
            throw new NotImplementedException(Resources.NotImplementedProjectionRepository);
        }

        public Task SaveAsync(Projection projection)
        {
            throw new NotImplementedException(Resources.NotImplementedProjectionRepository);
        }

        public void SetSequenceNumber(string projectionName, long sequenceNumber)
        {
            throw new NotImplementedException(Resources.NotImplementedProjectionRepository);
        }

        public Task SetSequenceNumberAsync(string projectionName, long sequenceNumber)
        {
            throw new NotImplementedException(Resources.NotImplementedProjectionRepository);
        }

        public long GetSequenceNumber(string projectionName)
        {
            throw new NotImplementedException(Resources.NotImplementedProjectionRepository);
        }

        public ValueTask<long> GetSequenceNumberAsync(string projectionName)
        {
            throw new NotImplementedException(Resources.NotImplementedProjectionRepository);
        }
    }
}