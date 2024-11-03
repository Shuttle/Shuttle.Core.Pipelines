using System;

namespace Shuttle.Core.Pipelines;

internal class InstancePipelineObserverProvider : IPipelineObserverProvider
{
    private readonly IPipelineObserver _pipelineObserver;
    private readonly Type _type;

    public InstancePipelineObserverProvider(IPipelineObserver pipelineObserver)
    {
        _pipelineObserver = pipelineObserver;
        _type = pipelineObserver.GetType();
    }

    public IPipelineObserver GetObserverInstance()
    {
        return _pipelineObserver;
    }

    public Type GetObserverType()
    {
        return _type;
    }
}