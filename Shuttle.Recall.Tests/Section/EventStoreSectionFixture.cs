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
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Format(@".\Section\files\{0}", file)));
        }

        [Test]
        [TestCase("EventStore.config")]
        [TestCase("EventStore-Grouped.config")]
        public void Should_be_able_to_load_a_valid_configuration(string file)
        {
            var section = Open(file);

            Assert.IsNotNull(section);
            Assert.AreEqual(TimeSpan.FromSeconds(1), section.DurationToSleepWhenIdle[0]);
        }

        [Test]
        public void Should_be_able_to_load_an_empty_configuration()
        {
            var section = Open("Empty.config");

            Assert.IsNull(section.DurationToSleepWhenIdle);
        }
    }
}