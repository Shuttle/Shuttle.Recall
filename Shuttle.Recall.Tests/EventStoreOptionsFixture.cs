using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Shuttle.Recall.Tests;

[TestFixture]
public class RecallOptionsFixture
{
    protected RecallOptions GetOptions()
    {
        var result = new RecallOptions();

        result.EventProcessing.ProjectionProcessorIdleDurations.Clear();

        new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @".\appsettings.json")).Build()
            .GetRequiredSection($"{RecallOptions.SectionName}").Bind(result);

        return result;
    }

    [Test]
    public void Should_be_able_to_add_and_check_for_active_projection_names()
    {
        var eventStoreOptions = new RecallOptions();

        Assert.That(eventStoreOptions.HasActiveProjection("projection-excluded"), Is.True);
        Assert.That(eventStoreOptions.HasActiveProjection("projection-included"), Is.True);

        eventStoreOptions.EventProcessing.IncludedProjections.Add("projection-included");

        Assert.That(eventStoreOptions.HasActiveProjection("projection-excluded"), Is.False);
        Assert.That(eventStoreOptions.HasActiveProjection("projection-included"), Is.True);

        eventStoreOptions.EventProcessing.IncludedProjections.Clear();
        eventStoreOptions.EventProcessing.ExcludedProjections.Add("projection-excluded");

        Assert.That(eventStoreOptions.HasActiveProjection("projection-excluded"), Is.False);
        Assert.That(eventStoreOptions.HasActiveProjection("projection-included"), Is.True);
    }

    [Test]
    public void Should_be_able_to_load_a_valid_configuration()
    {
        var options = GetOptions();

        Assert.That(options, Is.Not.Null);
        Assert.That(options.EventProcessing.ProjectionProcessorIdleDurations[0], Is.EqualTo(TimeSpan.FromSeconds(1)));

        Assert.That(options.EventProcessing.IncludedProjections, Is.Not.Null);
        Assert.That(options.EventProcessing.IncludedProjections.Count, Is.EqualTo(3));
    }
}