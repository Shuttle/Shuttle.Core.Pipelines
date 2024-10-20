namespace Shuttle.Core.Pipelines;

public class OnPipelineException : PipelineEvent
{
    public OnPipelineException(IPipeline pipeline) : base(pipeline)
    {
    }
}