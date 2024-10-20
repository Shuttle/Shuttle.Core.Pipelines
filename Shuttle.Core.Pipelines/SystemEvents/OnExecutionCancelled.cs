namespace Shuttle.Core.Pipelines;

public class OnExecutionCancelled : PipelineEvent
{
    public OnExecutionCancelled(IPipeline pipeline) : base(pipeline)
    {
    }
}