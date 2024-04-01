using System;
using System.Collections.Generic;

namespace Shuttle.Core.Pipelines.Tests;

public class AmbientData
{
    public string ActiveValue { get; private set; } = null;
    public IEnumerable<string> Values => _values.AsReadOnly();

    private readonly List<string> _values = new();

    public void Add(string value)
    {
        if (_values.Contains(value))
        {
            throw new InvalidOperationException();
        }

        _values.Add(value);
    }

    public void Activate(string value)
    {
        ActiveValue = value;
    }

    public void Remove(string value)
    {
        if (!_values.Contains(value))
        {
            throw new InvalidOperationException();
        }

        _values.Remove(value);
    }
}