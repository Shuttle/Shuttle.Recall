namespace Shuttle.Recall.Core
{
    public interface ICanSnapshot
    {
        object GetSnapshotEvent();
    }
}