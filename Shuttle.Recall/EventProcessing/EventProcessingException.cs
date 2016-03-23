using System;

namespace Shuttle.Recall
{
    public class EventProcessingException : Exception
    {
        public EventProcessingException(string message) : base(message)
        {
        }
    }
}