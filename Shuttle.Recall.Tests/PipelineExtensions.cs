using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall.Tests;

public static class PipelineExtensions
{
    extension(Pipeline)
    {
        public static Pipeline Get(ServiceProvider? serviceProvider = null)
        {
            var pipelineOptions = Options.Create(new PipelineOptions());

            return new(pipelineOptions, serviceProvider ?? new Mock<IServiceProvider>().Object);
        }
    }
}