using System;
using System.Collections.Generic;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Tests.EventProcessing
{
    public class FakeEventHandler :
        IEventHandler<FakeEvent1>,
        IEventHandler<FakeEvent2>
    {
        private readonly object _padlock = new object();
        private readonly List<string> _propertyValues = new List<string>();

        public void ProcessEvent(IEventHandlerContext<FakeEvent1> context)
        {
            Guard.AgainstNull(context, "domainEvent");

            lock (_padlock)
            {
                _propertyValues.Add(context.DomainEvent.PropertyOne);
            }
        }

        public void ProcessEvent(IEventHandlerContext<FakeEvent2> context)
        {
            Guard.AgainstNull(context, "domainEvent");

            lock (_padlock)
            {
                _propertyValues.Add(context.DomainEvent.PropertyTwo);
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