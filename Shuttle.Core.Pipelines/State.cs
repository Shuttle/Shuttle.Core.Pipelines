using System.Collections.Generic;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Pipelines;

public class State : IState
{
    private readonly Dictionary<string, object?> _state = new();

    public void Clear()
    {
        _state.Clear();
    }

    public void Add(string key, object? value)
    {
        _state.Add(Guard.AgainstNull(key), value);
    }

    public void Replace(string key, object? value)
    {
        Guard.AgainstNull(key);

        _state.Remove(key);
        _state.Add(key, value);
    }

    public object? Get(string key)
    {
        return _state.TryGetValue(Guard.AgainstNull(key), out var result) ? result : default;
    }

    public bool Contains(string key)
    {
        return _state.ContainsKey(Guard.AgainstNull(key));
    }

    public bool Remove(string key)
    {
        return _state.Remove(key);
    }
}