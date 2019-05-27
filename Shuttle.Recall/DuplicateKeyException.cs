using System;

namespace Shuttle.Recall
{
    public class DuplicateKeyException : Exception
    {
        public DuplicateKeyException(Guid id, string key)
        :base(string.Format(Resources.DuplicateKeyException, id, key))
        {
        }

        public DuplicateKeyException(string message) : base(message)
        {
        }

        public DuplicateKeyException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}