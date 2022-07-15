using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Shuttle.Core.Configuration;

namespace Shuttle.Recall.Tests
{
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

            Assert.IsNotNull(options);
            Assert.AreEqual(TimeSpan.FromSeconds(1), options.DurationToSleepWhenIdle[0]);

            Assert.That(options.ActiveProjections, Is.Not.Null);
            Assert.That(options.ActiveProjections.Count, Is.EqualTo(3));
        }
    }
}