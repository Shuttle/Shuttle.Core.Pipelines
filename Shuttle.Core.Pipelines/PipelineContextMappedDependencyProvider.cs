namespace Shuttle.Core.Pipelines;

internal class PipelineContextMappedDependencyProvider : IMappedDependencyProvider
{
    private readonly IPipelineContext _pipelineContext;

    public PipelineContextMappedDependencyProvider(IPipelineContext pipelineContext)
    {
        _pipelineContext = pipelineContext;
    }

    public object Get()
    {
        return _pipelineContext;
    }
}