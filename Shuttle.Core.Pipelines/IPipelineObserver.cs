using System.Threading.Tasks;

namespace Shuttle.Core.Pipelines;

public interface IPipelineObserver
{
}

public interface IPipelineObserver<TPipelineEvent> : IPipelineObserver where TPipelineEvent : class
{
    Task ExecuteAsync(IPipelineContext<TPipelineEvent> pipelineContext);
}