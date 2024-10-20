namespace Shuttle.Core.Pipelines;

public class OnStageCompleted : PipelineEvent
{
    public OnStageCompleted(IPipeline pipeline) : base(pipeline)
    {
    }
}