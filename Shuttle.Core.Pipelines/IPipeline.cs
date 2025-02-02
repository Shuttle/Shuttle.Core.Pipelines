using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shuttle.Core.Pipelines;

public interface IPipeline
{
    bool Aborted { get; }
    CancellationToken CancellationToken { get; }
    Exception? Exception { get; }
    bool ExceptionHandled { get; }

    Guid Id { get; }
    string StageName { get; }
    IState State { get; }
    void Abort();
    Task<bool> ExecuteAsync(CancellationToken cancellationToken = default);
    IPipelineStage GetStage(string name);
    void MarkExceptionHandled();

    event EventHandler<PipelineEventArgs> PipelineCompleted;
    event EventHandler<PipelineEventArgs> PipelineStarting;
    IPipeline AddObserver(IPipelineObserver pipelineObserver);
    IPipeline AddObserver(Type observerType);
    IPipeline AddObserver(Delegate handler);
    IPipelineStage AddStage(string name);
    event EventHandler<PipelineEventArgs> StageCompleted;
    event EventHandler<PipelineEventArgs> StageStarting;
    event EventHandler<PipelineEventArgs> PipelineRecursiveException;
}