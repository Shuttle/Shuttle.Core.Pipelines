using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Shuttle.Core.Pipelines.Tests;

[TestFixture]
public class PipelineFixture
{
    [Test]
    public async Task Should_be_able_to_execute_a_valid_pipeline_async()
    {
        var pipeline = new Pipeline();

        pipeline
            .RegisterStage("Stage")
            .WithEvent<MockPipelineEvent1>()
            .WithEvent<MockPipelineEvent2>()
            .WithEvent<MockPipelineEvent3>();

        var observer = new MockAuthenticateObserver();

        pipeline.RegisterObserver(observer);

        await pipeline.ExecuteAsync(CancellationToken.None);

        Assert.That(observer.CallSequence, Is.EqualTo("123"));
    }

    [Test]
    public async Task Should_be_able_to_register_events_after_existing_event_async()
    {
        var pipeline = new Pipeline();

        pipeline.RegisterStage("Stage")
            .WithEvent<MockPipelineEvent3>()
            .AfterEvent<MockPipelineEvent3>().Register<MockPipelineEvent2>()
            .AfterEvent<MockPipelineEvent2>().Register<MockPipelineEvent1>();

        var observer = new MockAuthenticateObserver();

        pipeline.RegisterObserver(observer);

        await pipeline.ExecuteAsync(CancellationToken.None);

        Assert.That(observer.CallSequence, Is.EqualTo("321"));
    }

    [Test]
    public async Task Should_be_able_to_register_events_before_existing_event_async()
    {
        var pipeline = new Pipeline();

        pipeline.RegisterStage("Stage")
            .WithEvent<MockPipelineEvent1>();

        pipeline.GetStage("Stage").BeforeEvent<MockPipelineEvent1>().Register<MockPipelineEvent2>();
        pipeline.GetStage("Stage").BeforeEvent<MockPipelineEvent2>().Register<MockPipelineEvent3>();

        var observer = new MockAuthenticateObserver();

        pipeline.RegisterObserver(observer);

        await pipeline.ExecuteAsync(CancellationToken.None);

        Assert.That(observer.CallSequence, Is.EqualTo("321"));
    }

    [Test]
    public void Should_fail_on_attempt_to_register_events_after_non_existent_event()
    {
        Assert.Throws<InvalidOperationException>(
            () =>
                new Pipeline()
                    .RegisterStage("Stage")
                    .AfterEvent<MockPipelineEvent1>()
                    .Register<MockPipelineEvent2>());
    }

    [Test]
    public void Should_fail_on_attempt_to_register_events_before_non_existent_event()
    {
        Assert.Throws<InvalidOperationException>(
            () =>
                new Pipeline()
                    .RegisterStage("Stage")
                    .BeforeEvent<MockPipelineEvent1>()
                    .Register<MockPipelineEvent2>());
    }

    [Test]
    public async Task Should_be_able_to_call_an_interfaced_observer_async()
    {
        var pipeline = new Pipeline();

        pipeline.RegisterStage("Stage")
            .WithEvent<MockPipelineEvent1>();

        var interfacedObserver = new InterfacedObserver();

        pipeline.RegisterObserver(interfacedObserver);

        await pipeline.ExecuteAsync(CancellationToken.None);

        Assert.That(interfacedObserver.Called, Is.True);
    }

    [Test]
    public void Should_be_able_to_use_scoped_ambient_context_state_async()
    {
        var ambientDataService = new AmbientDataService();
        var pipeline = new AmbientDataPipeline(ambientDataService);

        Assert.That(async () =>
        {
            ambientDataService.BeginScope();

            await pipeline.ExecuteAsync(CancellationToken.None);
        }, Throws.Nothing);
    }

    [Test]
    public void Should_be_not_able_to_use_ambient_context_state_without_scope_async()
    {
        var ambientDataService = new AmbientDataService();
        var pipeline = new AmbientDataPipeline(ambientDataService);

        Assert.That(async () =>
        {
            await pipeline.ExecuteAsync(CancellationToken.None);
        }, Throws.TypeOf<PipelineException>());
    }
}