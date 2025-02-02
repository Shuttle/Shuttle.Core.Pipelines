using System;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Pipelines;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPipelineProcessing(this IServiceCollection services, Action<PipelineProcessingBuilder>? builder = null)
    {
        Guard.AgainstNull(services);

        var pipelineProcessingBuilder = new PipelineProcessingBuilder(services);

        builder?.Invoke(pipelineProcessingBuilder);

        services.AddOptions<PipelineOptions>().Configure(options =>
        {
            options.ReusePipelines = pipelineProcessingBuilder.Options.ReusePipelines;
        });

        services.TryAddSingleton<IPipelineFactory, PipelineFactory>();

        return services;
    }
}