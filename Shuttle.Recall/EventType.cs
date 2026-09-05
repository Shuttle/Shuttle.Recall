using Shuttle.Contract;

namespace Shuttle.Recall;

public class EventType(Guid id, string typeName)
{
    public Guid Id { get; } = Guard.AgainstEmpty(id);
    public string TypeName { get; } = Guard.AgainstEmpty(typeName);

    public class Specification
    {
        private readonly List<Guid> _ids = [];
        public string TypeNameMatch { get; private set; } = string.Empty;

        public Specification WithTypeNameMatch(string typeNameMatch)
        {
            TypeNameMatch = Guard.AgainstEmpty(typeNameMatch);
            return this;
        }

        public Specification AddId(Guid id)
        {
            Guard.AgainstNull(id);

            if (!_ids.Contains(id))
            {
                _ids.Add(id);
            }

            return this;
        }

        public Specification AddIds(IEnumerable<Guid> ids)
        {
            foreach (var id in ids)
            {
                AddId(id);
            }

            return this;
        }
    }
}