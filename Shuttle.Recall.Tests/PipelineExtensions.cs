using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Shuttle.Core.Pipelines;
using Shuttle.Core.TransactionScope;

namespace Shuttle.Recall.Tests;

public static class PipelineExtensions
{
    extension(Pipeline)
    {
        public static Pipeline Get(ServiceProvider? serviceProvider = null)
        {
            var pipelineOptions = Options.Create(new PipelineOptions());
            var transactionScopeOptions = Options.Create(new TransactionScopeOptions());

            return new(pipelineOptions, transactionScopeOptions, new TransactionScopeFactory(transactionScopeOptions), serviceProvider ?? new Mock<IServiceProvider>().Object);
        }
    }
}