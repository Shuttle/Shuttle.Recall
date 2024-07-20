namespace Shuttle.Recall
{
    public static class ProjectionConfigurationExtensions
    {
        public static void AddProjectionEventHandlerType<T>(this IProjectionConfiguration configuration, string projectionName)
        {
            configuration.AddProjectionEventHandlerType(projectionName, typeof(T));
        }
    }
}