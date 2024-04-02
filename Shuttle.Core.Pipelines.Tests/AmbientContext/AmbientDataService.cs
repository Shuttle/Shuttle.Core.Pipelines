using System;
using System.Linq;
using System.Threading;

namespace Shuttle.Core.Pipelines.Tests;

public class AmbientDataService : IAmbientDataService
{
    private static AsyncLocal<AmbientData> _ambientData;

    public AmbientDataService()
    {
        _ambientData = new AsyncLocal<AmbientData>();
    }

    private readonly SemaphoreSlim _lock = new(1, 1);

    public string Current
    {
        get
        {
            _lock.Wait();

            try
            {
                return GetAmbientData().ActiveValue ?? throw new InvalidOperationException();
            }
            finally
            {
                _lock.Release();
            }
        }
    }

    public void Activate(string value)
    {
        _lock.Wait();

        try
        {
            var current = GetAmbientData().ActiveValue;

            if (current != null && current.Equals(value))
            {
                throw new Exception($"Value '{value}' already added.");
            }

            var activate = GetAmbientData().Values.FirstOrDefault(item => item.Equals(value, StringComparison.InvariantCultureIgnoreCase));

            if (activate == null)
            {
                throw new Exception($"Value '{value}' not found.");
            }

            GetAmbientData().Activate(activate);
        }
        finally
        {
            _lock.Release();
        }
    }

    public void Add(string value)
    {
        _lock.Wait();

        try
        {
            GetAmbientData().Add(value);
        }
        finally
        {
            _lock.Release();
        }

        Activate(value);
    }

    public void Remove(string value)
    {
        _lock.Wait();

        try
        {
            GetAmbientData().Remove(value);
        }
        finally
        {
            _lock.Release();
        }
    }

    private AmbientData GetAmbientData()
    {
        if (_ambientData.Value == null)
        {
            _ambientData.Value = new AmbientData();
        }

        return _ambientData.Value;
    }

    public void BeginScope()
    {
        GetAmbientData();
    }
}