using System;
using System.Collections.Generic;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Core.Tests.EventProcessing
{
    public class FakeEventHandler :
        IEventHandler<FakeEvent1>,
        IEventHandler<FakeEvent2>
    {
        private readonly object _padlock = new object();
        private readonly List<string> _propertyValues = new List<string>();

        public void ProcessEvent(FakeEvent1 domainEvent)
        {
            Guard.AgainstNull(domainEvent, "domainEvent");

            lock (_padlock)
            {
                _propertyValues.Add(domainEvent.PropertyOne);
            }
        }

        public void ProcessEvent(FakeEvent2 domainEvent)
        {
            Guard.AgainstNull(domainEvent, "domainEvent");

            lock (_padlock)
            {
                _propertyValues.Add(domainEvent.PropertyTwo);
            }
        }

        public bool HasValue(string propertyValue)
        {
            lock (_padlock)
            {
                return
                    _propertyValues
                        .Find(value => value.Equals(propertyValue, StringComparison.InvariantCultureIgnoreCase)) != null;
            }
        }
    }
}