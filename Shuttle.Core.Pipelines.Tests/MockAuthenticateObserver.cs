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

        public async Task Execute(MockPipelineEvent1 pipelineEvent)
        {
            Console.WriteLine(@"[collection] : {0}", pipelineEvent.Name);

            CallSequence += "1";
            
            await Task.CompletedTask;
        }

        public async Task Execute(MockPipelineEvent2 pipelineEvent)
        {
            Console.WriteLine(@"[collection] : {0}", pipelineEvent.Name);

            CallSequence += "2";

            await Task.CompletedTask;
        }

        public async Task Execute(MockPipelineEvent3 pipelineEvent)
        {
            Console.WriteLine(@"[collection] : {0}", pipelineEvent.Name);

            CallSequence += "3";

            await Task.CompletedTask;
        }
    }
}