using System;
using Shuttle.Core.Contract;

namespace Shuttle.Recall
{
    public class ProjectionProvider : IProjectionProvider
    {
        private readonly IProjectionRepository _repository;

        public ProjectionProvider(IProjectionRepository repository)
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
                projection = new Projection(name, 1, Environment.MachineName, AppDomain.CurrentDomain.BaseDirectory);

                _repository.Save(projection);
            }

            return projection;
        }
    }
}