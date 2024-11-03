using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Shuttle.Core.Pipelines;

internal class MappedDelegate
{
    private readonly IEnumerable<Type> _parameterTypes;

    public MappedDelegate(Delegate handler, IEnumerable<Type> parameterTypes)
    {
        Handler = handler;
        HasParameters = parameterTypes.Any();
        _parameterTypes = parameterTypes;
    }

    public Delegate Handler { get; }
    public bool HasParameters { get; }

    public object[] GetParameters(IServiceProvider serviceProvider, object pipelineContext)
    {
        return _parameterTypes
            .Select(parameterType => !parameterType.IsCastableTo(typeof(IPipelineContext))
                ? serviceProvider.GetRequiredService(parameterType)
                : pipelineContext
            ).ToArray();
    }
}