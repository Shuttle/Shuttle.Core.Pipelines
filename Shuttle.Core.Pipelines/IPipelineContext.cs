using System;

namespace Shuttle.Core.Pipelines;

public interface IPipelineContext<T>
{
    IPipeline Pipeline { get; }
    Type EventType { get; }
}