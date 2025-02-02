using System;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Pipelines;

public class PipelineExceptionEventArgs : EventArgs
{
    public PipelineExceptionEventArgs(IPipeline pipeline)
    {
        Pipeline = Guard.AgainstNull(pipeline);
    }

    public IPipeline Pipeline { get; }
}