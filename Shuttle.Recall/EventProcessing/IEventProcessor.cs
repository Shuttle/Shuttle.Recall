using System;
using System.Threading.Tasks;

namespace Shuttle.Recall;

public interface IEventProcessor : IDisposable, IAsyncDisposable
{
    bool Started { get; }
    Task<IEventProcessor> StartAsync();
    Task StopAsync();
}