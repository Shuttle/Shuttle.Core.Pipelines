using System;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Pipelines;

public class PipelineContextEventArgs : EventArgs
{
    public PipelineContextEventArgs(IPipelineContext pipelineContext)
    {
        PipelineContext = Guard.AgainstNull(pipelineContext);
    }

    public IPipelineContext PipelineContext { get; }
}