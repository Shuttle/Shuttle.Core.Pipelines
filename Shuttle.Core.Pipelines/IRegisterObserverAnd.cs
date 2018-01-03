namespace Shuttle.Core.Pipelines
{
    public interface IRegisterObserverAnd
    {
        IRegisterObserverAnd AndObserver(IPipelineObserver pipelineObserver);
    }
}