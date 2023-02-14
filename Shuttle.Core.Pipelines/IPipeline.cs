using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shuttle.Core.Pipelines
{
    public interface IPipeline
    {
        Guid Id { get; }
        CancellationToken CancellationToken { get; }
        bool ExceptionHandled { get; }
        Exception Exception { get; }
        bool Aborted { get; }
        string StageName { get; }
        IPipelineEvent Event { get; }
        IState State { get; }
        void Abort();
        void MarkExceptionHandled();
        Task<bool> Execute(CancellationToken cancellationToken);
        IPipelineStage RegisterStage(string name);
        IPipelineStage GetStage(string name);
        IPipeline RegisterObserver(IPipelineObserver pipelineObserver);
    }
}