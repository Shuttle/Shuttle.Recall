using System;
using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Contract;

namespace Shuttle.Recall;

public static class EventEnvelopeExtensions
{
    public static void AcceptInvariants(this EventEnvelope eventEnvelope)
    {
        Guard.AgainstNull(eventEnvelope.EventId);
        Guard.AgainstNullOrEmptyString(eventEnvelope.AssemblyQualifiedName);
    }

    public static bool CompressionEnabled(this EventEnvelope eventEnvelope)
    {
        return !string.IsNullOrEmpty(Guard.AgainstNull(eventEnvelope).CompressionAlgorithm)
               &&
               !eventEnvelope.CompressionAlgorithm.Equals("none", StringComparison.InvariantCultureIgnoreCase);
    }

    public static bool Contains(this IEnumerable<EnvelopeHeader> headers, string key)
    {
        return Guard.AgainstNull(headers).Any(header => header.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase));
    }

    public static bool EncryptionEnabled(this EventEnvelope eventEnvelope)
    {
        return !string.IsNullOrEmpty(Guard.AgainstNull(eventEnvelope).EncryptionAlgorithm)
               &&
               !eventEnvelope.EncryptionAlgorithm.Equals("none", StringComparison.InvariantCultureIgnoreCase);
    }

    public static string GetHeaderValue(this List<EnvelopeHeader> headers, string key)
    {
        var header = Guard.AgainstNull(headers).FirstOrDefault(candidate => candidate.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase));

        return header == null ? string.Empty : header.Value;
    }

    public static void Merge(this List<EnvelopeHeader> merge, IEnumerable<EnvelopeHeader> headers)
    {
        Guard.AgainstNull(merge);

        foreach (var header in Guard.AgainstNull(headers).Where(header => !merge.Contains(header.Key)))
        {
            merge.Add(new()
            {
                Key = header.Key,
                Value = header.Value
            });
        }
    }
}