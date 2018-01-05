namespace Shuttle.Core.Pipelines.Tests
{
    public class InterfacedObserver : IInterfacedObserver
    {
        public bool Called { get; private set; }
        
        public void Execute(MockPipelineEvent1 pipelineEvent)
        {
            Called = true;
        }
    }

    public interface IInterfacedObserver : IPipelineObserver<MockPipelineEvent1>
    {
    }
}