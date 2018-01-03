using System;

namespace Shuttle.Core.Pipelines
{
    public class PipelineEventArgs : EventArgs
    {
        public PipelineEventArgs(IPipeline pipeline)
        {
            Pipeline = pipeline;
        }

        public IPipeline Pipeline { get; }
    }
}