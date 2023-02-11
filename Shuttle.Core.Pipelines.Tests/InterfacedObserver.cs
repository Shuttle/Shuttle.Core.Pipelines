using System.Threading.Tasks;

namespace Shuttle.Core.Pipelines.Tests
{
    public class InterfacedObserver : IInterfacedObserver
    {
        public bool Called { get; private set; }
        
        public async Task Execute(MockPipelineEvent1 pipelineEvent)
        {
            Called = true;

            await Task.CompletedTask;
        }
    }

    public interface IInterfacedObserver : IPipelineObserver<MockPipelineEvent1>
    {
    }
}