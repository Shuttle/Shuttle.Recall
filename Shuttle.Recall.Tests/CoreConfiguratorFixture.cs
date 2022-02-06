using System;
using System.Configuration;
using System.Transactions;
using NUnit.Framework;

namespace Shuttle.Recall.Tests
{
    [TestFixture]
    public class CoreConfiguratorFixture
    {
        [Test]
        public void Should_be_able_to_populate_configuration()
        {
            Console.WriteLine(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).FilePath);

            var configuration = new EventStoreConfiguration();
            var configurator = new CoreConfigurator();

            configurator.Apply(configuration);

            Assert.IsNotNull(configuration.DurationToSleepWhenIdle);
            Assert.AreEqual(TimeSpan.FromSeconds(1), configuration.DurationToSleepWhenIdle[0]);
            Assert.AreEqual("compression-algorithm", configuration.CompressionAlgorithm);
            Assert.AreEqual("encryption-algorithm", configuration.EncryptionAlgorithm);

            Assert.IsFalse(configuration.TransactionScope.Enabled);
            Assert.AreEqual(IsolationLevel.RepeatableRead, configuration.TransactionScope.IsolationLevel);
            Assert.AreEqual(300, configuration.TransactionScope.TimeoutSeconds);
        }
    }
}