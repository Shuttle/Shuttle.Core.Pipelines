using System;

namespace Shuttle.Core.Pipelines.Tests
{
    public class MockAuthenticateObserver :
        IPipelineObserver<MockPipelineEvent1>,
        IPipelineObserver<MockPipelineEvent2>,
        IPipelineObserver<MockPipelineEvent3>
    {
        public string CallSequence { get; private set; } = string.Empty;

        public void Execute(MockPipelineEvent1 pipelineEvent)
        {
            Console.WriteLine(@"[collection] : {0}", pipelineEvent.Name);

            CallSequence += "1";
        }

        public void Execute(MockPipelineEvent2 pipelineEvent)
        {
            Console.WriteLine(@"[collection] : {0}", pipelineEvent.Name);

            CallSequence += "2";
        }

        public void Execute(MockPipelineEvent3 pipelineEvent)
        {
            Console.WriteLine(@"[collection] : {0}", pipelineEvent.Name);

            CallSequence += "3";
        }
    }
}