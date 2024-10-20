namespace Shuttle.Core.Pipelines;

public class OnStageStarting : PipelineEvent
{
    public OnStageStarting(IPipeline pipeline) : base(pipeline)
    {
    }
}