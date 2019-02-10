namespace Shuttle.Recall
{
    public interface IProjectionProvider
    {
        Projection Get(string name);
    }
}