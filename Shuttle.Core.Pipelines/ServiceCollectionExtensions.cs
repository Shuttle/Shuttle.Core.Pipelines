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
        private static readonly List<Type> FeatureTypes = new List<Type>();

        public static IServiceCollection AddPipelineProcessing(this IServiceCollection services,
            Action<PipelineProcessingBuilder> builder = null)
        {
            Guard.AgainstNull(services, nameof(services));

            var pipelineProcessingBuilder = new PipelineProcessingBuilder(services);

            builder?.Invoke(pipelineProcessingBuilder);

            services.TryAddSingleton<IPipelineFactory, PipelineFactory>();

            services.AddSingleton<IPipelineFeatureProvider>(serviceProvider => new PipelineFeatureProvider(FeatureTypes));

            return services;
        }

        public static IServiceCollection AddPipelineFeature<T>(this IServiceCollection services) where T : class
        {
            return AddPipelineFeature(services, typeof(T));
        }

        public static IServiceCollection AddPipelineFeature(this IServiceCollection services, Type type)
        {
            Guard.AgainstNull(services, nameof(services));
            Guard.AgainstNull(type, nameof(type));

            if (!FeatureTypes.Contains(type))
            {
                FeatureTypes.Add(type);
            }

            services.TryAddSingleton(type, type);

            return services;
        }
    }
}