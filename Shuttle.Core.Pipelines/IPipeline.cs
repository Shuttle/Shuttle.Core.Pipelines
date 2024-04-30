using System;
using System.Threading.Tasks;
using System.Threading;

namespace Shuttle.Core.Pipelines
{
    public interface IPipeline
    {
        Guid Id { get; }
        bool ExceptionHandled { get; }
        Exception Exception { get; }
        bool Aborted { get; }
        string StageName { get; }
        IPipelineEvent Event { get; }
        IState State { get; }
        void Abort();
        void MarkExceptionHandled();
        IPipelineStage RegisterStage(string name);
        IPipelineStage GetStage(string name);
        CancellationToken CancellationToken { get; }
        bool Execute(CancellationToken cancellationToken = default);
        Task<bool> ExecuteAsync(CancellationToken cancellationToken = default);
        IPipeline RegisterObserver(IPipelineObserver pipelineObserver);
    }
}