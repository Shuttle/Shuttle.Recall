using Shuttle.Contract;

namespace Shuttle.Recall.Query;

public class EventType
{
    public Guid Id { get; set; }
    public string TypeName { get; set; } = string.Empty;

    public Recall.EventType ToActual()
    {
        return new(Id, TypeName);
    }

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