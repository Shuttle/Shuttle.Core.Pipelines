using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shuttle.Core.Contract;
using Shuttle.Core.Reflection;

namespace Shuttle.Core.Pipelines
{
    public static class ServiceCollectionExtensions
    {
        private static readonly List<Type> ModuleTypes = new List<Type>();

        public static IServiceCollection AddPipelineProcessing(this IServiceCollection services, Assembly assembly)
        {
            Guard.AgainstNull(services, nameof(services));
            Guard.AgainstNull(assembly, nameof(assembly));

            services.TryAddSingleton<IPipelineFactory, PipelineFactory>();

            var reflectionService = new ReflectionService();

            foreach (var type in reflectionService.GetTypesAssignableTo<IPipeline>(assembly))
            {
                if (type.IsInterface || type.IsAbstract || services.Contains(ServiceDescriptor.Transient(type, type)))
                {
                    continue;
                }

                services.AddTransient(type, type);
            }

            AddPipelineObservers(services, assembly);

            services.AddSingleton<IPipelineModuleProvider>(serviceProvider => new PipelineModuleProvider(ModuleTypes));

            return services;
        }

        public static IServiceCollection AddPipelineObservers(this IServiceCollection services, Assembly assembly)
        {
            Guard.AgainstNull(services, nameof(services));
            Guard.AgainstNull(assembly, nameof(assembly));

            var reflectionService = new ReflectionService();

            foreach (var type in reflectionService.GetTypesAssignableTo<IPipelineObserver>(assembly))
            {
                if (type.IsInterface || type.IsAbstract)
                {
                    continue;
                }

                var interfaceType = type.InterfaceMatching($"I{type.Name}");

                if (interfaceType != null)
                {
                    if (services.Contains(ServiceDescriptor.Transient(interfaceType, type)))
                    {
                        continue;
                    }

                    services.AddTransient(interfaceType, type);
                }
                else
                {
                    throw new InvalidOperationException(string.Format(Resources.ObserverInterfaceMissingException, type.Name));
                }
            }

            return services;
        }

        public static IServiceCollection AddPipelineModule<T>(this IServiceCollection services) where T : class
        {
            return AddPipelineModule(services, typeof(T));
        }

        public static IServiceCollection AddPipelineModule(this IServiceCollection services, Type type)
        {
            Guard.AgainstNull(services, nameof(services));
            Guard.AgainstNull(type, nameof(type));

            if (!ModuleTypes.Contains(type))
            {
                ModuleTypes.Add(type);
            }

            services.TryAddSingleton(type, type);

            return services;
        }
    }
}