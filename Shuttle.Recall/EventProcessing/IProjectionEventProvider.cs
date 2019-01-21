namespace Shuttle.Recall
{
    public interface IProjectionEventProvider
    {
        ProjectionEvent Get(Projection projection);
    }
}