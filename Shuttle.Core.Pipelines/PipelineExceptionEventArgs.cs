using Shuttle.Core.Contract;
using System;

namespace Shuttle.Core.Pipelines
{
    public class PipelineExceptionEventArgs : EventArgs
    {
        public PipelineExceptionEventArgs(IPipeline pipeline)
        {
            Pipeline = Guard.AgainstNull(pipeline, nameof(pipeline));
        }

        public IPipeline Pipeline { get; }
    }
}