using System;

namespace Shuttle.Recall
{
    public interface IEventProcessor : IDisposable
    {
		IEventProcessor Start();
        void Stop();

        bool Started { get; }
        void AddEventProjection(EventProjection eventProjection);
	}
}