using System.CodeDom;
using NUnit.Framework;

namespace Shuttle.Recall.Tests
{
    [TestFixture]
    public class PrimitiveEventCacheFixture
    {
        [Test]
        public void Should_be_able_to_use_cache()
        {
            var projectionName = "test";
            var cache = new PrimitiveEventQueue();
            var primitiveEvent = new PrimitiveEvent();

            Assert.IsNull(cache.Dequeue(projectionName));

            cache.Enqueue(projectionName, primitiveEvent);

            var dequeued = cache.Dequeue(projectionName);

            Assert.IsNotNull(dequeued);
            Assert.AreEqual(primitiveEvent.Id, dequeued.Id);
            Assert.IsNull(cache.Dequeue(projectionName));
        }
    }
}