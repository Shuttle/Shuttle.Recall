using NUnit.Framework;

namespace Shuttle.Recall.Tests
{
    [TestFixture]
    public class ProjectionSequenceNumberTrackerFixture
    {
        [Test]
        public void Should_be_able_to_use_tracker()
        {
            var tracker = new ProjectionSequenceNumberTracker();

            Assert.IsFalse(tracker.Contains("projection"));
            Assert.IsNull(tracker.TryGet("projection"));

            tracker.Set("projection", 100);

            Assert.IsTrue(tracker.Contains("projection"));
            Assert.AreEqual(100, tracker.TryGet("projection"));
        }
    }
}