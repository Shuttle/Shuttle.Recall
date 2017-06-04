using System.Collections.Generic;
using NUnit.Framework;
using Shuttle.Recall.Tests.Implementation;

namespace Shuttle.Recall.Tests
{
    [TestFixture]
    public class DefaultEventMethodInvokerFixture
    {
        [Test]
        public void Should_be_able_to_invoke_public_methods()
        {
            var invoker = new DefaultEventMethodInvoker(new EventMethodInvokerConfiguration
            {
                AllowPublicMethod = false
            });

            var events = new List<object>
            {
                new ThisHappened {ThisValue = "this"},
                new ThatHappened {ThatValue = "that"}
            };

            var aggregateOne = new AggregateOne();

            Assert.IsNull(aggregateOne.ThisValue);
            Assert.IsNull(aggregateOne.ThatValue);

            invoker.Apply(aggregateOne, events);

            Assert.AreEqual("this", aggregateOne.ThisValue);
            Assert.AreEqual("that", aggregateOne.ThatValue);

            var aggregateTwo = new AggregateTwo();

            Assert.IsNull(aggregateTwo.ThisValue);
            Assert.IsNull(aggregateTwo.ThatValue);

            invoker.Apply(aggregateTwo, events);

            Assert.AreEqual("this", aggregateTwo.ThisValue);
            Assert.AreEqual("that", aggregateTwo.ThatValue);
        }
    }
}