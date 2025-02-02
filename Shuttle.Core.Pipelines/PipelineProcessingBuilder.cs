﻿using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;
using Shuttle.Core.Reflection;

namespace Shuttle.Core.Pipelines;

public class PipelineProcessingBuilder
{
    public PipelineProcessingBuilder(IServiceCollection services)
    {
        Services = Guard.AgainstNull(services);
    }

    public IServiceCollection Services { get; }

    public PipelineOptions Options
    {
        get => _pipelineOptions;
        set => _pipelineOptions = value ?? throw new ArgumentNullException(nameof(value));
    }

    private PipelineOptions _pipelineOptions = new();

    public PipelineProcessingBuilder AddAssembly(Assembly assembly)
    {
        Guard.AgainstNull(assembly);

        var reflectionService = new ReflectionService();

        foreach (var type in reflectionService.GetTypesCastableToAsync<IPipeline>(assembly).GetAwaiter().GetResult())
        {
            if (type.IsInterface || type.IsAbstract || Services.Contains(ServiceDescriptor.Transient(type, type)))
            {
                continue;
            }

            Services.AddTransient(type, type);
        }

        foreach (var type in reflectionService.GetTypesCastableToAsync<IPipelineObserver>(assembly).GetAwaiter().GetResult())
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