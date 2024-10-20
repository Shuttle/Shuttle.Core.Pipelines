namespace Shuttle.Core.Pipelines;

public class OnAbortPipeline : PipelineEvent
{
    public OnAbortPipeline(IPipeline pipeline) : base(pipeline)
    {
    }
}