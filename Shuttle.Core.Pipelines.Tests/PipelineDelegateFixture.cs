using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace Shuttle.Core.Pipelines.Tests;

internal static class PipelineExtensions
{
    private static void CallSequence(IPipelineContext pipelineContext, string number)
    {
        pipelineContext.Pipeline.State.Replace("CallSequence", (pipelineContext.Pipeline.State.Get<string>("CallSequence") ?? string.Empty) + number);
    }

    public static void MapObservers(this Pipeline pipeline)
    {
        pipeline.MapObserver<MockPipelineEvent1>(async (IPipelineContext<MockPipelineEvent1> pipelineContext) =>
        {
            CallSequence(pipelineContext,"1");

            await Task.CompletedTask;
        });

        pipeline.MapObserver<MockPipelineEvent2>(async (IPipelineContext<MockPipelineEvent2> pipelineContext) =>
        {
            CallSequence(pipelineContext, "2");

            await Task.CompletedTask;
        });

        pipeline.MapObserver<MockPipelineEvent3>(async (IPipelineContext<MockPipelineEvent3> pipelineContext) =>
        {
            CallSequence(pipelineContext, "3");

            await Task.CompletedTask;
        });
    }
}

[TestFixture]
public class PipelineDelegateFixture
{
    [Test]
    public async Task Should_be_able_to_execute_a_type_based_pipeline_async()
    {
        var services = new ServiceCollection();

        services.AddSingleton<MockAuthenticateObserver>();

        var serviceProvider = services.BuildServiceProvider();

        var pipeline = new Pipeline(serviceProvider);

        pipeline
            .RegisterStage("Stage")
            .WithEvent<MockPipelineEvent1>()
            .WithEvent<MockPipelineEvent2>()
            .WithEvent<MockPipelineEvent3>();

        var callSequence = string.Empty;

        pipeline.MapObserver<MockPipelineEvent1>(async () =>
        {
            callSequence += "1";
            await Task.CompletedTask;
        });

        pipeline.MapObserver<MockPipelineEvent2>(async () =>
        {
            callSequence += "2";
            await Task.CompletedTask;
        });

        pipeline.MapObserver<MockPipelineEvent3>(async () =>
        {
            callSequence += "3";
            await Task.CompletedTask;
        });

        await pipeline.ExecuteAsync(CancellationToken.None);

        Assert.That(callSequence, Is.EqualTo("123"));
    }

    [Test]
    public async Task Should_be_able_to_register_events_after_existing_event_async()
    {
        var pipeline = new Pipeline(new Mock<IServiceProvider>().Object);

        pipeline.RegisterStage("Stage")
            .WithEvent<MockPipelineEvent3>()
            .AfterEvent<MockPipelineEvent3>().Register<MockPipelineEvent2>()
            .AfterEvent<MockPipelineEvent2>().Register<MockPipelineEvent1>();

        pipeline.MapObservers();

        await pipeline.ExecuteAsync(CancellationToken.None);

        Assert.That(pipeline.State.Get<string>("CallSequence"), Is.EqualTo("321"));
    }

    [Test]
    public async Task Should_be_able_to_register_events_before_existing_event_async()
    {
        var pipeline = new Pipeline(new Mock<IServiceProvider>().Object);

        pipeline.RegisterStage("Stage")
            .WithEvent<MockPipelineEvent1>();

        pipeline.GetStage("Stage").BeforeEvent<MockPipelineEvent1>().Register<MockPipelineEvent2>();
        pipeline.GetStage("Stage").BeforeEvent<MockPipelineEvent2>().Register<MockPipelineEvent3>();

        pipeline.MapObservers();

        await pipeline.ExecuteAsync(CancellationToken.None);

        Assert.That(pipeline.State.Get<string>("CallSequence"), Is.EqualTo("321"));
    }

    [Test]
    public async Task Should_not_be_able_to_register_delegate_requesting_wrong_event_type()
    {
        var pipeline = new Pipeline(new Mock<IServiceProvider>().Object);

        pipeline.RegisterStage("Stage")
            .WithEvent<MockPipelineEvent1>();

        Assert.That(() =>
        {
            pipeline.MapObserver<MockPipelineEvent2>(async (IPipelineContext<MockPipelineEvent1> _) =>
            {
                await Task.CompletedTask;
            });
        }, Throws.Exception);
    }
}