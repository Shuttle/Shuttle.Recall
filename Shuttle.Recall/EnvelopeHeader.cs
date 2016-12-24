using System;

namespace Shuttle.Recall
{
    [Serializable]
    public class EnvelopeHeader
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}