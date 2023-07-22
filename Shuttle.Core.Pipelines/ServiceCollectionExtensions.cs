using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shuttle.Core.Contract;
using Shuttle.Core.Reflection;

namespace Shuttle.Core.Pipelines
{
    public static class ServiceCollectionExtensions
    {
        private static readonly Type PipelineFeatureType = typeof(IPipelineFeature);

        public static IServiceCollection AddPipelineFeature<T>(this IServiceCollection services) where T : IPipelineFeature
        {
            return services.AddPipelineFeature(typeof(T));
        }

        public static IServiceCollection AddPipelineFeature(this IServiceCollection services, Type type)
        {
            Guard.AgainstNull(services, nameof(services));
            Guard.AgainstNull(type, nameof(type));

            if (!type.IsAssignableTo(PipelineFeatureType))
            {
                throw new ArgumentException(string.Format(Resources.PipelineFeatureTypeException, type.Name));
            }

            services.AddSingleton(PipelineFeatureType, type);

            return services;
        }

        public static IServiceCollection AddPipelineProcessing(this IServiceCollection services,
            Action<PipelineProcessingBuilder> builder = null)
        {
            Guard.AgainstNull(services, nameof(services));

            var pipelineProcessingBuilder = new PipelineProcessingBuilder(services);

            builder?.Invoke(pipelineProcessingBuilder);

            services.TryAddSingleton<IPipelineFactory, PipelineFactory>();

            return services;
        }
    }
}