using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;
using Shuttle.Core.Reflection;

namespace Shuttle.Core.Pipelines;

public class PipelineProcessingBuilder
{
    public PipelineProcessingBuilder(IServiceCollection services)
    {
        Services = Guard.AgainstNull(services, nameof(services));
    }

    public IServiceCollection Services { get; }

    public async Task<PipelineProcessingBuilder> AddAssemblyAsync(Assembly assembly)
    {
        Guard.AgainstNull(assembly, nameof(assembly));

        var reflectionService = new ReflectionService();

        foreach (var type in await reflectionService.GetTypesAssignableToAsync<IPipeline>(assembly))
        {
            if (type.IsInterface || type.IsAbstract || Services.Contains(ServiceDescriptor.Transient(type, type)))
            {
                continue;
            }

            Services.AddTransient(type, type);
        }

        foreach (var type in await reflectionService.GetTypesAssignableToAsync<IPipelineObserver>(assembly))
        {
            if (type.IsInterface || type.IsAbstract)
            {
                continue;
            }

            var interfaceType = type.InterfaceMatching($"I{type.Name}");

            if (interfaceType != null)
            {
                if (Services.Contains(ServiceDescriptor.Singleton(interfaceType, type)))
                {
                    continue;
                }

                Services.AddSingleton(interfaceType, type);
            }
            else
            {
                throw new InvalidOperationException(string.Format(Resources.ObserverInterfaceMissingException, type.Name));
            }
        }

        return this;
    }
}