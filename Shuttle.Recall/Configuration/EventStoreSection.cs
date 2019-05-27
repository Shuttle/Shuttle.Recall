using System;
using System.ComponentModel;
using System.Configuration;
using Shuttle.Core.Configuration;
using Shuttle.Core.TimeSpanTypeConverters;

namespace Shuttle.Recall
{
    public class EventStoreSection : ConfigurationSection
    {
        private static bool _initialized;
        private static readonly object Padlock = new object();
        private static EventStoreSection _section;

        [ConfigurationProperty("encryptionAlgorithm", IsRequired = false, DefaultValue = "")]
        public string EncryptionAlgorithm => (string) this["encryptionAlgorithm"];

        [ConfigurationProperty("compressionAlgorithm", IsRequired = false, DefaultValue = "")]
        public string CompressionAlgorithm => (string) this["compressionAlgorithm"];

        [ConfigurationProperty("projectionEventFetchCount", IsRequired = false, DefaultValue = 100)]
        public int ProjectionEventFetchCount => (int) this["projectionEventFetchCount"];

        [ConfigurationProperty("projectionThreadCount", IsRequired = false, DefaultValue = 5)]
        public int ProjectionThreadCount => (int) this["projectionThreadCount"];

        [TypeConverter(typeof(StringDurationArrayConverter))]
        [ConfigurationProperty("durationToSleepWhenIdle", IsRequired = false, DefaultValue = null)]
        public TimeSpan[] DurationToSleepWhenIdle => (TimeSpan[]) this["durationToSleepWhenIdle"];

        [ConfigurationProperty("activeProjections", IsRequired = false, DefaultValue = null)]
        public ActiveProjectionElementCollection ActiveProjections => (ActiveProjectionElementCollection)this["activeProjections"];

        [ConfigurationProperty("sequenceNumberTailThreadWorkerInterval", IsRequired = false, DefaultValue = 5000)]
        public int SequenceNumberTailThreadWorkerInterval { get; set; }

        public static EventStoreSection Get()
        {
            lock (Padlock)
            {
                if (!_initialized)
                {
                    _section = ConfigurationSectionProvider.Open<EventStoreSection>("shuttle", "eventStore");

                    _initialized = true;
                }

                return _section;
            }
        }
    }
}