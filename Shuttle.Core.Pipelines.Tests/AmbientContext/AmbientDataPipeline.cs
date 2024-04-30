namespace Shuttle.Core.Pipelines.Tests;

public class AmbientDataPipeline : Pipeline
{
    public AmbientDataPipeline(IAmbientDataService ambientDataService)
    {
        RegisterStage("Pipeline")
            .WithEvent<OnAddValue>()
            .WithEvent<OnGetValue>()
            .WithEvent<OnRemoveValue>();

        RegisterObserver(new AmbientDataObserver(ambientDataService));
    }
}