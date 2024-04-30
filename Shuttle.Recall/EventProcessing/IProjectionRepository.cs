using System.Threading.Tasks;

namespace Shuttle.Recall
{
    public interface IProjectionRepository
    {
        Projection Find(string name);
        Task<Projection> FindAsync(string name);
        void Save(Projection projection);
        Task SaveAsync(Projection projection);
        void SetSequenceNumber(string projectionName, long sequenceNumber);
        Task SetSequenceNumberAsync(string projectionName, long sequenceNumber);
        long GetSequenceNumber(string projectionName);
        ValueTask<long> GetSequenceNumberAsync(string projectionName);
    }
}