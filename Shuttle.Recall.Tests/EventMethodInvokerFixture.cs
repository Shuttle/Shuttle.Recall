using System.Collections.Generic;
using NUnit.Framework;
using Shuttle.Recall.Tests.Implementation;

namespace Shuttle.Recall.Tests;

[TestFixture]
public class EventMethodInvokerFixture
{
    [Test]
    public void Should_be_able_to_invoke_public_methods()
    {
        var invoker = new EventMethodInvoker(new EventMethodInvokerConfiguration());

        var events = new List<object>
        {
            new ThisHappened { ThisValue = "this" },
            new ThatHappened { ThatValue = "that" }
        };

        var aggregateOne = new AggregateOne();

        Assert.That(aggregateOne.ThisValue, Is.EqualTo(string.Empty));
        Assert.That(aggregateOne.ThatValue, Is.EqualTo(string.Empty));

        invoker.Apply(aggregateOne, events);

        Assert.That(aggregateOne.ThisValue, Is.EqualTo("this"));
        Assert.That(aggregateOne.ThatValue, Is.EqualTo("that"));

        var aggregateTwo = new AggregateTwo();

        Assert.That(aggregateTwo.ThisValue, Is.EqualTo(string.Empty));
        Assert.That(aggregateTwo.ThatValue, Is.EqualTo(string.Empty));

        invoker.Apply(aggregateTwo, events);

        Assert.That(aggregateTwo.ThisValue, Is.EqualTo("this"));
        Assert.That(aggregateTwo.ThatValue, Is.EqualTo("that"));
    }
}