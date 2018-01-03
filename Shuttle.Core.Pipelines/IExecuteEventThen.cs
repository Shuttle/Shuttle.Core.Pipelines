namespace Shuttle.Core.Pipelines
{
    public interface IExecuteEventThen
    {
        IExecuteEventThen ThenEvent(IPipelineEvent pipelineEvent);
        IExecuteEventThen ThenEvent<TPipelineEvent>() where TPipelineEvent : IPipelineEvent, new();
    }
}