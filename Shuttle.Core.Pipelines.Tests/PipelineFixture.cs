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

        if (sync)
        {
            pipeline.Execute();
        }
        else
        {
            await pipeline.ExecuteAsync(CancellationToken.None);
        }

        Assert.AreEqual("123", observer.CallSequence);
    }

    [Test]
    public void Should_be_able_to_register_events_after_existing_event()
    {
        Should_be_able_to_register_events_after_existing_event_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_register_events_after_existing_event_async()
    {
        await Should_be_able_to_register_events_after_existing_event_async(false);
    }

    private async Task Should_be_able_to_register_events_after_existing_event_async(bool sync)
    {
        var pipeline = new Pipeline();

        pipeline.RegisterStage("Stage")
            .WithEvent<MockPipelineEvent3>()
            .AfterEvent<MockPipelineEvent3>().Register<MockPipelineEvent2>()
            .AfterEvent<MockPipelineEvent2>().Register(new MockPipelineEvent1());

        var observer = new MockAuthenticateObserver();

        pipeline.RegisterObserver(observer);

        if (sync)
        {
            pipeline.Execute();
        }
        else
        {
            await pipeline.ExecuteAsync(CancellationToken.None);
        }

        Assert.AreEqual("321", observer.CallSequence);
    }

    [Test]
    public void Should_be_able_to_register_events_before_existing_event()
    {
        Should_be_able_to_register_events_before_existing_event_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_register_events_before_existing_event_async()
    {
        await Should_be_able_to_register_events_before_existing_event_async(false);
    }

    private async Task Should_be_able_to_register_events_before_existing_event_async(bool sync)
    {
        var pipeline = new Pipeline();

        pipeline.RegisterStage("Stage")
            .WithEvent<MockPipelineEvent1>();

        pipeline.GetStage("Stage").BeforeEvent<MockPipelineEvent1>().Register<MockPipelineEvent2>();
        pipeline.GetStage("Stage").BeforeEvent<MockPipelineEvent2>().Register(new MockPipelineEvent3());

        var observer = new MockAuthenticateObserver();

        pipeline.RegisterObserver(observer);

        if (sync)
        {
            pipeline.Execute();
        }
        else
        {
            await pipeline.ExecuteAsync(CancellationToken.None);
        }

        Assert.AreEqual("321", observer.CallSequence);
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
    public void Should_not_be_able_to_register_a_null_event()
    {
        var pipeline = new Pipeline();

        Assert.Throws<NullReferenceException>(() => pipeline.RegisterStage("Stage").WithEvent(null));
    }

    [Test]
    public void Should_be_able_to_call_an_interfaced_observer()
    {
        Should_be_able_to_call_an_interfaced_observer_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_call_an_interfaced_observer_async()
    {
        await Should_be_able_to_call_an_interfaced_observer_async(false);
    }

    private async Task Should_be_able_to_call_an_interfaced_observer_async(bool sync)
    {
        var pipeline = new Pipeline();

        pipeline.RegisterStage("Stage")
            .WithEvent<MockPipelineEvent1>();

        var interfacedObserver = new InterfacedObserver();

        pipeline.RegisterObserver(interfacedObserver);

        if (sync)
        {
            pipeline.Execute();
        }
        else
        {
            await pipeline.ExecuteAsync(CancellationToken.None);
        }

        Assert.IsTrue(interfacedObserver.Called);
    }

    [Test]
    public void Should_be_able_to_use_scoped_ambient_context_state()
    {
        Should_be_able_to_use_scoped_ambient_context_state_async(true);
    }

    [Test]
    public void Should_be_able_to_use_scoped_ambient_context_state_async()
    {
        Should_be_able_to_use_scoped_ambient_context_state_async(false);
    }

    private void Should_be_able_to_use_scoped_ambient_context_state_async(bool sync)
    {
        var ambientDataService = new AmbientDataService();
        var pipeline = new AmbientDataPipeline(ambientDataService);

        Assert.That(async () =>
        {
            ambientDataService.BeginScope();

            if (sync)
            {
                pipeline.Execute();
            }
            else
            {
                await pipeline.ExecuteAsync(CancellationToken.None);
            }
        }, Throws.Nothing);
    }

    [Test]
    public void Should_be_not_able_to_use_ambient_context_state_without_scope()
    {
        Should_be_not_able_to_use_ambient_context_state_without_scope_async(true);
    }

    [Test]
    public void Should_be_not_able_to_use_ambient_context_state_without_scope_async()
    {
        Should_be_not_able_to_use_ambient_context_state_without_scope_async(false);
    }

    private void Should_be_not_able_to_use_ambient_context_state_without_scope_async(bool sync)
    {
        var ambientDataService = new AmbientDataService();
        var pipeline = new AmbientDataPipeline(ambientDataService);

        Assert.That(async () =>
        {
            if (sync)
            {
                pipeline.Execute();
            }
            else
            {
                await pipeline.ExecuteAsync(CancellationToken.None);
            }
        }, Throws.TypeOf<PipelineException>());
    }
}