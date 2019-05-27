using System;
using System.IO;
using NUnit.Framework;
using Shuttle.Core.Configuration;

namespace Shuttle.Recall.Tests
{
    [TestFixture]
    public class EventStoreSectionFixture
    {
        private EventStoreSection Open(string file)
        {
            return ConfigurationSectionProvider.OpenFile<EventStoreSection>("shuttle", "eventStore",
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@".\Section\files\{file}"));
        }

        [Test]
        [TestCase("EventStore.config")]
        [TestCase("EventStore-Grouped.config")]
        public void Should_be_able_to_load_a_valid_configuration(string file)
        {
            var section = Open(file);

            Assert.IsNotNull(section);
            Assert.AreEqual(TimeSpan.FromSeconds(1), section.DurationToSleepWhenIdle[0]);

            Assert.That(section.ActiveProjections, Is.Not.Null);
            Assert.That(section.ActiveProjections.Count, Is.EqualTo(3));
        }

        [Test]
        public void Should_be_able_to_load_an_empty_configuration()
        {
            var section = Open("Empty.config");

            Assert.IsNull(section.DurationToSleepWhenIdle);
            Assert.That(section.ActiveProjections, Is.Empty);
            Assert.That(section.ActiveProjections.Count, Is.Zero);
        }
    }
}