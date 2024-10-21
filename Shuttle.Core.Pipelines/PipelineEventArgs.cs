using System;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Pipelines;

public class PipelineEventArgs : EventArgs
{
    public PipelineEventArgs(IPipeline pipeline)
    {
        Pipeline = Guard.AgainstNull(pipeline);
    }

    public IPipeline Pipeline { get; }
}