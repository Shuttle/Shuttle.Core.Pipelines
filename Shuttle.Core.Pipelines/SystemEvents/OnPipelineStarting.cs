namespace Shuttle.Core.Pipelines;

public class OnPipelineStarting : PipelineEvent
{
    public OnPipelineStarting(IPipeline pipeline) : base(pipeline)
    {
    }
}