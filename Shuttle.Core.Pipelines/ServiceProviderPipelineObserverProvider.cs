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
        if (_serviceProvider.GetService(_type) is not IPipelineObserver result)
        {
            throw new InvalidOperationException(string.Format(Resources.MissingPipelineObserverException, _type.FullName));
        }

        return result;
    }

    public Type GetObserverType()
    {
        return _type;
    }
}