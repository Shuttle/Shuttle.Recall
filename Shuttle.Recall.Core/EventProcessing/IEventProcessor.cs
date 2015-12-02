using System;

namespace Shuttle.Recall.Core
{
    public interface IEventProcessor : IDisposable
    {
        void Start();
        void Stop();

        bool Started { get; }
        void AddEventProjector(IEventProjector eventProjector);
    }
}