using System;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Pipelines;

public class PipelineEventEventArgs : EventArgs
{
    public PipelineEventEventArgs(IPipelineEvent pipelineEvent)
    {
        PipelineEvent = Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent));
    }

    public IPipelineEvent PipelineEvent { get; }
}