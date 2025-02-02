using System;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Shuttle.Recall.Tests;

[TestFixture]
public class EventStoreOptionsFixture
{
    protected EventStoreOptions GetOptions()
    {
        var result = new EventStoreOptions();

        result.DurationToSleepWhenIdle.Clear();

        new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @".\appsettings.json")).Build()
            .GetRequiredSection($"{EventStoreOptions.SectionName}").Bind(result);

        return result;
    }

    [Test]
    public void Should_be_able_to_load_a_valid_configuration()
    {
        var options = GetOptions();

        Assert.That(options, Is.Not.Null);
        Assert.That(options.DurationToSleepWhenIdle[0], Is.EqualTo(TimeSpan.FromSeconds(1)));

        Assert.That(options.ActiveProjections, Is.Not.Null);
        Assert.That(options.ActiveProjections.Count, Is.EqualTo(3));
    }

    [Test]
    public void Should_be_able_to_load_a_processor_thread_options()
    {
        var options = GetOptions();

        Assert.That(options, Is.Not.Null);
        Assert.That(options.ProcessorThread.IsBackground, Is.False);
        Assert.That(options.ProcessorThread.JoinTimeout, Is.EqualTo(TimeSpan.FromSeconds(15)));
        Assert.That(options.ProcessorThread.Priority, Is.EqualTo(ThreadPriority.Lowest));
    }
}