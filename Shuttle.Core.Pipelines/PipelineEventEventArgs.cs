using System;

namespace Shuttle.Core.Pipelines
{
    public class PipelineEventEventArgs : EventArgs
    {
        public PipelineEventEventArgs(IPipelineEvent pipelineEvent)
        {
            PipelineEvent = pipelineEvent;
        }

        public IPipelineEvent PipelineEvent { get; }
    }
}