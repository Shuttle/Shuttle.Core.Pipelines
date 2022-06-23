using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Shuttle.Core.Pipelines.Tests
{
    [TestFixture]
    public class PipelineFixture
    {
        [Test]
        public void Should_be_able_to_execute_a_valid_pipeline()
        {
            var pipeline = GetPipeline();

            pipeline
                .RegisterStage("Stage")
                .WithEvent<MockPipelineEvent1>()
                .WithEvent<MockPipelineEvent2>()
                .WithEvent<MockPipelineEvent3>();

            var observer = new MockAuthenticateObserver();

            pipeline.RegisterObserver(observer);

            pipeline.Execute();

            Assert.AreEqual("123", observer.CallSequence);
        }

        private static Pipeline GetPipeline()
        {
            return new Pipeline(new Logger<Pipeline>(new NullLoggerFactory()));
        }

        [Test]
        public void Should_be_able_to_register_events_after_existing_event()
        {
            var pipeline = GetPipeline();

            pipeline.RegisterStage("Stage")
                .WithEvent<MockPipelineEvent3>()
                .AfterEvent<MockPipelineEvent3>().Register<MockPipelineEvent2>()
                .AfterEvent<MockPipelineEvent2>().Register(new MockPipelineEvent1());

            var observer = new MockAuthenticateObserver();

            pipeline.RegisterObserver(observer);

            pipeline.Execute();

            Assert.AreEqual("321", observer.CallSequence);
        }

        [Test]
        public void Should_be_able_to_register_events_before_existing_event()
        {
            var pipeline = GetPipeline();

            pipeline.RegisterStage("Stage")
                .WithEvent<MockPipelineEvent1>();

            pipeline.GetStage("Stage").BeforeEvent<MockPipelineEvent1>().Register<MockPipelineEvent2>();
            pipeline.GetStage("Stage").BeforeEvent<MockPipelineEvent2>().Register(new MockPipelineEvent3());

            var observer = new MockAuthenticateObserver();

            pipeline.RegisterObserver(observer);

            pipeline.Execute();

            Assert.AreEqual("321", observer.CallSequence);
        }

        [Test]
        public void Should_fail_on_attempt_to_register_events_after_non_existent_event()
        {
            Assert.Throws<InvalidOperationException>(
                () =>
                    GetPipeline().RegisterStage("Stage")
                        .AfterEvent<MockPipelineEvent1>()
                        .Register<MockPipelineEvent2>());
        }

        [Test]
        public void Should_fail_on_attempt_to_register_events_before_non_existent_event()
        {
            Assert.Throws<InvalidOperationException>(
                () =>
                    GetPipeline().RegisterStage("Stage")
                        .BeforeEvent<MockPipelineEvent1>()
                        .Register<MockPipelineEvent2>());
        }

        [Test]
        public void Should_not_be_able_to_register_a_null_event()
        {
            var pipeline = GetPipeline();

            Assert.Throws<NullReferenceException>(() => pipeline.RegisterStage("Stage").WithEvent(null));
        }

        [Test]
        public void Should_be_able_to_call_an_interfaced_observer()
        {
            var pipeline = GetPipeline();

            pipeline.RegisterStage("Stage")
                .WithEvent<MockPipelineEvent1>();

            var interfacedObserver = new InterfacedObserver();
            
            pipeline.RegisterObserver(interfacedObserver);

            pipeline.Execute();

            Assert.IsTrue(interfacedObserver.Called);
        }
    }
}