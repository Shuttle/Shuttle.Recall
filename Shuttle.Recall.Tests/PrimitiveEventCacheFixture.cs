using NUnit.Framework;

namespace Shuttle.Recall.Tests
{
    [TestFixture]
    public class PrimitiveEventCacheFixture
    {
        [Test]
        public void Should_be_able_to_use_cache()
        {
            var cache = new PrimitiveEventCache();
            var primitiveEvent = new PrimitiveEvent();

            Assert.IsNull(cache.TryGet(1));

            cache.Add(primitiveEvent);

            Assert.IsNotNull(cache.TryGet(1));
            Assert.AreEqual(primitiveEvent.Id, cache.TryGet(1).Id);
        }
    }
}