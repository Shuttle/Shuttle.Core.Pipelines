using System;
using System.Threading.Tasks;

namespace Shuttle.Core.Pipelines.Tests;

public class AmbientDataObserver :
    IPipelineObserver<OnAddValue>,
    IPipelineObserver<OnGetValue>,
    IPipelineObserver<OnRemoveValue>
{
    private readonly IAmbientDataService _ambientDataService;

    public AmbientDataObserver(IAmbientDataService ambientDataService)
    {
        _ambientDataService = ambientDataService;
    }

    public async Task ExecuteAsync(IPipelineContext<OnAddValue> pipelineContext)
    {
        _ambientDataService.Add("A");

        await Task.CompletedTask;
    }

    public async Task ExecuteAsync(IPipelineContext<OnGetValue> pipelineContext)
    {
        Console.WriteLine(_ambientDataService.Current);

        await Task.CompletedTask;
    }

    public async Task ExecuteAsync(IPipelineContext<OnRemoveValue> pipelineContext)
    {
        _ambientDataService.Remove("A");

        await Task.CompletedTask;
    }
}

public class OnRemoveValue
{
}

public class OnGetValue
{
}

public class OnAddValue
{
}