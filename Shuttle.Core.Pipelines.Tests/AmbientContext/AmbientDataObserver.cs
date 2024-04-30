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
    
    public void Execute(OnAddValue pipelineEvent)
    {
        ExecuteAsync(pipelineEvent).GetAwaiter().GetResult();
    }

    public async Task ExecuteAsync(OnAddValue pipelineEvent)
    {
        _ambientDataService.Add("A");

        await Task.CompletedTask;
    }

    public void Execute(OnGetValue pipelineEvent)
    {
        ExecuteAsync(pipelineEvent).GetAwaiter().GetResult();
    }

    public async Task ExecuteAsync(OnGetValue pipelineEvent)
    {
        Console.WriteLine(_ambientDataService.Current);

        await Task.CompletedTask;
    }

    public void Execute(OnRemoveValue pipelineEvent)
    {
        ExecuteAsync(pipelineEvent).GetAwaiter().GetResult();
    }

    public async Task ExecuteAsync(OnRemoveValue pipelineEvent)
    {
        _ambientDataService.Remove("A");

        await Task.CompletedTask;
    }
}

public class OnRemoveValue : PipelineEvent
{
}

public class OnGetValue : PipelineEvent
{
}

public class OnAddValue : PipelineEvent
{
}