using Shuttle.Core.Contract;

namespace Shuttle.Core.Pipelines;

public abstract class PipelineEvent
{
    protected PipelineEvent(IPipeline pipeline)
    {
        Pipeline = Guard.AgainstNull(pipeline, nameof(pipeline));
    }

    public IPipeline Pipeline { get; }
}