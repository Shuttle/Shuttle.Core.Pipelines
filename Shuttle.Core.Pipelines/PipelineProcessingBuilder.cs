using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;
using Shuttle.Core.Reflection;

namespace Shuttle.Core.Pipelines
{
    public class PipelineProcessingBuilder
    {
        public IServiceCollection Services { get; }
        public PipelineProcessingBuilder(IServiceCollection services)
        {
            Services = Guard.AgainstNull(services, nameof(services));
        }

        public PipelineProcessingBuilder AddAssembly(Assembly assembly)
        {
            Guard.AgainstNull(assembly, nameof(assembly));

            var reflectionService = new ReflectionService();

            foreach (var type in reflectionService.GetTypesAssignableTo<IPipeline>(assembly).GetAwaiter().GetResult())
            {
                if (type.IsInterface || type.IsAbstract || Services.Contains(ServiceDescriptor.Transient(type, type)))
                {
                    continue;
                }

                Services.AddTransient(type, type);
            }

            foreach (var type in reflectionService.GetTypesAssignableTo<IPipelineObserver>(assembly).GetAwaiter().GetResult())
            {
                if (type.IsInterface || type.IsAbstract)
                {
                    continue;
                }

                var interfaceType = type.InterfaceMatching($"I{type.Name}");

                if (interfaceType != null)
                {
                    if (Services.Contains(ServiceDescriptor.Transient(interfaceType, type)))
                    {
                        continue;
                    }

                    Services.AddTransient(interfaceType, type);
                }
                else
                {
                    throw new InvalidOperationException(string.Format(Resources.ObserverInterfaceMissingException, type.Name));
                }
            }

            return this;
        }
    }
}