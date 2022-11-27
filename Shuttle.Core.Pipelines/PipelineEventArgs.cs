using Shuttle.Core.Contract;
using System;

namespace Shuttle.Core.Pipelines
{
    public class PipelineEventArgs : EventArgs
    {
        public PipelineEventArgs(IPipeline pipeline)
        {
            Pipeline = Guard.AgainstNull(pipeline, nameof(pipeline));
        }

        public IPipeline Pipeline { get; }
    }
}