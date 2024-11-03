using System;
using System.Collections.Generic;
using System.Linq;

namespace Shuttle.Core.Pipelines;

internal class MappedDelegate
{
    private readonly IEnumerable<IMappedDependencyProvider> _providers;

    public MappedDelegate(Delegate handler, IEnumerable<IMappedDependencyProvider> providers)
    {
        _providers = providers;
        Handler = handler;
        HasArgs = providers.Any();
    }

    public object[] GetArgs()
    {
        var result = new List<object>();

        foreach (var provider in _providers)
        {
            result.Add(provider.Get());
        }

        return result.ToArray();
    }

    public Delegate Handler { get; }
    public bool HasArgs { get; }
}