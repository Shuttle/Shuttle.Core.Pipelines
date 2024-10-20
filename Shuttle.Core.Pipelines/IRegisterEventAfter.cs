namespace Shuttle.Core.Pipelines;

public interface IRegisterEventAfter
{
    IPipelineStage Register<TPipelineEvent>() where TPipelineEvent : class, new();
}