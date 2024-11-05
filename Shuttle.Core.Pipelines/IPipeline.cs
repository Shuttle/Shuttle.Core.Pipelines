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
    IPipeline RegisterObserver(IPipelineObserver pipelineObserver);
    IPipeline RegisterObserver(Type observerType);
    IPipeline MapObserver(Delegate handler);
    IPipelineStage RegisterStage(string name);
    event EventHandler<PipelineEventArgs> StageCompleted;
    event EventHandler<PipelineEventArgs> StageStarting;
}