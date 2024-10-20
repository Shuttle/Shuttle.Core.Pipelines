using System;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Pipelines;

public class PipelineContext<T> : IPipelineContext<T>
{
    public PipelineContext(IPipeline pipeline)
    {
        Pipeline = Guard.AgainstNull(pipeline);
    }

    public IPipeline Pipeline { get; }
    public Type EventType => typeof(T);
}