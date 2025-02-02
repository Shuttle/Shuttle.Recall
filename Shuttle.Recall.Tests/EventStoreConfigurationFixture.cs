using NUnit.Framework;

namespace Shuttle.Recall.Tests;

[TestFixture]
public class EventStoreConfigurationFixture
{
    [Test]
    public void Should_be_able_to_add_and_check_for_active_projection_names()
    {
        var eventStoreOptions = new EventStoreOptions();

        Assert.That(eventStoreOptions.HasActiveProjection("not-registered"), Is.True);

        eventStoreOptions.ActiveProjections.Add("projection-1");

        Assert.That(eventStoreOptions.HasActiveProjection("not-registered"), Is.False);
        Assert.That(eventStoreOptions.HasActiveProjection("projection-1"), Is.True);
    }
}