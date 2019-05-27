using NUnit.Framework;

namespace Shuttle.Recall.Tests
{
    [TestFixture]
    public class EventStoreConfigurationFixture
    {
        [Test]
        public void Should_be_able_to_add_and_check_for_active_projection_names()
        {
            var configuration = new EventStoreConfiguration();

            Assert.That(configuration.HasActiveProjection("not-registered"), Is.True);

            configuration.AddActiveProjectionName("projection-1");

            Assert.That(configuration.HasActiveProjection("not-registered"), Is.False);
            Assert.That(configuration.HasActiveProjection("projection-1"), Is.True);
        }
    }
}