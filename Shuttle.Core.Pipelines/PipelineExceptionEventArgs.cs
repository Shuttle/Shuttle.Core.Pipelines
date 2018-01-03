using System;

namespace Shuttle.Core.Pipelines
{
    public class PipelineExceptionEventArgs : EventArgs
    {
        public PipelineExceptionEventArgs(IPipeline pipeline)
        {
            Pipeline = pipeline;
        }

        public IPipeline Pipeline { get; }
    }
}