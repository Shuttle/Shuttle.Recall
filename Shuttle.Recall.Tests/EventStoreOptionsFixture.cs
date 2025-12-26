using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Shuttle.Recall.Tests;

[TestFixture]
public class EventStoreOptionsFixture
{
    protected EventStoreOptions GetOptions()
    {
        var result = new EventStoreOptions();

        result.ProjectionProcessorIdleDurations.Clear();

        new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @".\appsettings.json")).Build()
            .GetRequiredSection($"{EventStoreOptions.SectionName}").Bind(result);

        return result;
    }

    [Test]
    public void Should_be_able_to_add_and_check_for_active_projection_names()
    {
        var eventStoreOptions = new EventStoreOptions();

        Assert.That(eventStoreOptions.HasActiveProjection("not-registered"), Is.True);

        eventStoreOptions.ActiveProjections.Add("projection-1");

        Assert.That(eventStoreOptions.HasActiveProjection("not-registered"), Is.False);
        Assert.That(eventStoreOptions.HasActiveProjection("projection-1"), Is.True);
    }

    [Test]
    public void Should_be_able_to_load_a_valid_configuration()
    {
        var options = GetOptions();

        Assert.That(options, Is.Not.Null);
        Assert.That(options.ProjectionProcessorIdleDurations[0], Is.EqualTo(TimeSpan.FromSeconds(1)));

        Assert.That(options.ActiveProjections, Is.Not.Null);
        Assert.That(options.ActiveProjections.Count, Is.EqualTo(3));
    }
}