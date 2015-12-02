using System;

namespace Shuttle.Recall.Core
{
    public class EventProcessingException : Exception
    {
        public EventProcessingException(string message) : base(message)
        {
        }
    }
}