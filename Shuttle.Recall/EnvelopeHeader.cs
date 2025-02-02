using System;

namespace Shuttle.Recall;

[Serializable]
public class EnvelopeHeader
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}