using System;
using System.Runtime.Serialization;

namespace Shuttle.Recall
{
    public class EventStreamConcurrencyException : Exception
    {
        public EventStreamConcurrencyException()
        {
        }

        public EventStreamConcurrencyException(string message) : base(message)
        {
        }

        public EventStreamConcurrencyException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected EventStreamConcurrencyException(SerializationInfo info, StreamingContext context) : base(info,
            context)
        {
        }
    }
}