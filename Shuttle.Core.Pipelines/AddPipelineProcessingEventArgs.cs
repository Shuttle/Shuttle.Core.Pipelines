using Shuttle.Core.Contract;

namespace Shuttle.Core.Pipelines;

public class AddPipelineProcessingEventArgs : System.EventArgs
{
    public PipelineProcessingBuilder PipelineProcessingBuilder { get; }

    public AddPipelineProcessingEventArgs(PipelineProcessingBuilder pipelineProcessingBuilder)
    {
        PipelineProcessingBuilder = Guard.AgainstNull(pipelineProcessingBuilder);
    }
}