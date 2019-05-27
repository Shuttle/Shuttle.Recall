using System;
using System.Runtime.Serialization;

namespace Shuttle.Recall
{
    public class AggregateConstructorException : Exception
    {
        public AggregateConstructorException()
        {
        }

        public AggregateConstructorException(string message) : base(message)
        {
        }

        public AggregateConstructorException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AggregateConstructorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}