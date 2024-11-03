using System;

namespace Shuttle.Core.Pipelines;

internal class ServiceProviderPipelineObserverProvider : IPipelineObserverProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Type _type;

    public ServiceProviderPipelineObserverProvider(IServiceProvider serviceProvider, Type type)
    {
        _serviceProvider = serviceProvider;
        _type = type;
    }

    public IPipelineObserver GetObserverInstance()
    {
        return (IPipelineObserver)_serviceProvider.GetService(_type)!;
    }

    public Type GetObserverType()
    {
        return _type;
    }
}