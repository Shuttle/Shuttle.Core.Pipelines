namespace Shuttle.Core.Pipelines;

public interface IPipelineEvent
{
    IPipeline Pipeline { get; }
}