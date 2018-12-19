using Shuttle.Core.Contract;

namespace Shuttle.Recall
{
    public class ProjectionService : IProjectionService
    {
        private readonly IProjectionRepository _repository;

        public ProjectionService(IProjectionRepository repository)
        {
            Guard.AgainstNull(repository, nameof(repository));

            _repository = repository;
        }

        public Projection Get(string name)
        {
            Guard.AgainstNullOrEmptyString(name, nameof(name));

            var projection = _repository.Find(name);

            if (projection == null)
            {
                projection = new Projection(name, 1);

                _repository.Save(projection);
            }

            return projection;
        }
    }
}