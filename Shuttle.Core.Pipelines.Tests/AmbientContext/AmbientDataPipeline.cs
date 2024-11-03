using System;

namespace Shuttle.Core.Pipelines.Tests;

public class AmbientDataPipeline : Pipeline
{
    public AmbientDataPipeline(IServiceProvider serviceProvider, IAmbientDataService ambientDataService) : base(serviceProvider)
    {
        RegisterStage("Pipeline")
            .WithEvent<OnAddValue>()
            .WithEvent<OnGetValue>()
            .WithEvent<OnRemoveValue>();

        RegisterObserver(new AmbientDataObserver(ambientDataService));
    }
}