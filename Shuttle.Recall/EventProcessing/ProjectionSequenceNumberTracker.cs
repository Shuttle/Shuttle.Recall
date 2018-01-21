using System.Collections.Generic;
using Shuttle.Core.Contract;

namespace Shuttle.Recall
{
    public class ProjectionSequenceNumberTracker : IProjectionSequenceNumberTracker
    {
        private readonly object _padlock = new object();
        private readonly Dictionary<string, long> _sequenceNumbers = new Dictionary<string, long>();

        public void Set(string projectionName, long sequenceNumber)
        {
            Guard.AgainstNullOrEmptyString(projectionName, "projectionName");

            lock (_padlock)
            {
                _sequenceNumbers[projectionName] = sequenceNumber;
            }
        }

        public bool Contains(string projectionName)
        {
            Guard.AgainstNullOrEmptyString(projectionName, "projectionName");

            lock (_padlock)
            {
                return _sequenceNumbers.ContainsKey(projectionName);
            }
        }

        public long? TryGet(string projectionName)
        {
            Guard.AgainstNullOrEmptyString(projectionName, "projectionName");

            lock (_padlock)
            {
                return _sequenceNumbers.ContainsKey(projectionName)
                    ? (long?) _sequenceNumbers[projectionName]
                    : null;
            }
        }
    }
}