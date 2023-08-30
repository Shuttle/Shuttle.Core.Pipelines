using System.Threading.Tasks;

namespace Shuttle.Core.Pipelines.Tests
{
    public class InterfacedObserver : IInterfacedObserver
    {
        public bool Called { get; private set; }
        
        public Task ExecuteAsync(MockPipelineEvent1 pipelineEvent)
        {
            Called = true;

            return Task.CompletedTask;
        }

        public void Execute(MockPipelineEvent1 pipelineEvent)
        {
            Called = true;
        }
    }

    public interface IInterfacedObserver : IPipelineObserver<MockPipelineEvent1>
    {
    }
}