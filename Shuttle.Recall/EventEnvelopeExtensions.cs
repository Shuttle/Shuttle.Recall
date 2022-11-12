using System;
using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Contract;
using Enumerable = System.Linq.Enumerable;

namespace Shuttle.Recall
{
    public static class EventEnvelopeExtensions
    {
        public static bool EncryptionEnabled(this EventEnvelope eventEnvelope)
        {
            return !string.IsNullOrEmpty(eventEnvelope.EncryptionAlgorithm)
                   &&
                   !eventEnvelope.EncryptionAlgorithm.Equals("none", StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool CompressionEnabled(this EventEnvelope eventEnvelope)
        {
            return !string.IsNullOrEmpty(eventEnvelope.CompressionAlgorithm)
                   &&
                   !eventEnvelope.CompressionAlgorithm.Equals("none", StringComparison.InvariantCultureIgnoreCase);
        }

        public static void AcceptInvariants(this EventEnvelope eventEnvelope)
        {
            Guard.AgainstNull(eventEnvelope.EventId, nameof(eventEnvelope.EventId));
            Guard.AgainstNullOrEmptyString(eventEnvelope.AssemblyQualifiedName, nameof(eventEnvelope.AssemblyQualifiedName));
        }

        public static void Merge(this List<EnvelopeHeader> merge, IEnumerable<EnvelopeHeader> headers)
        {
            Guard.AgainstNull(headers, nameof(headers));

            foreach (var header in Enumerable.Where(headers, header => !merge.Contains(header.Key)))
            {
                merge.Add(new EnvelopeHeader
                {
                    Key = header.Key,
                    Value = header.Value
                });
            }
        }

        public static string GetHeaderValue(this List<EnvelopeHeader> headers, string key)
        {
            if (headers == null)
            {
                return string.Empty;
            }

            var header =
                headers.FirstOrDefault(
                    candidate => candidate.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase));

            return header == null ? string.Empty : header.Value;
        }

        public static bool Contains(this IEnumerable<EnvelopeHeader> headers, string key)
        {
            Guard.AgainstNull(headers, nameof(headers));

            return headers.Any(header => header.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}