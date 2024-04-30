using System;
using System.Threading.Tasks;

namespace Shuttle.Core.Pipelines.Tests
{
    public class MockAuthenticateObserver :
        IPipelineObserver<MockPipelineEvent1>,
        IPipelineObserver<MockPipelineEvent2>,
        IPipelineObserver<MockPipelineEvent3>
    {
        public string CallSequence { get; private set; } = string.Empty;

        private void Execute(PipelineEvent pipelineEvent, int delta)
        {
            Console.WriteLine(@"[collection] : {0}", pipelineEvent.Name);

            CallSequence += delta.ToString();
        }

        public void Execute(MockPipelineEvent1 pipelineEvent)
        {
            Execute(pipelineEvent, 1);
        }

        public async Task ExecuteAsync(MockPipelineEvent1 pipelineEvent)
        {
            Execute(pipelineEvent, 1);
            
            await Task.CompletedTask;
        }

        public void Execute(MockPipelineEvent2 pipelineEvent)
        {
            Execute(pipelineEvent, 2);
        }

        public async Task ExecuteAsync(MockPipelineEvent2 pipelineEvent)
        {
            Execute(pipelineEvent, 2);

            await Task.CompletedTask;
        }

        public void Execute(MockPipelineEvent3 pipelineEvent)
        {
            Execute(pipelineEvent, 3);
        }

        public async Task ExecuteAsync(MockPipelineEvent3 pipelineEvent)
        {
            Execute(pipelineEvent, 3);

            await Task.CompletedTask;
        }
    }
}