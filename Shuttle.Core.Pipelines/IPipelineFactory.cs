using System;

namespace Shuttle.Core.Pipelines
{
    public interface IPipelineFactory
    {
        event EventHandler<PipelineEventArgs> PipelineCreated;
        event EventHandler<PipelineEventArgs> PipelineObtained;
        event EventHandler<PipelineEventArgs> PipelineReleased;
        event EventHandler<FeaturesResolvedEventArgs> FeaturesResolved;

        TPipeline GetPipeline<TPipeline>() where TPipeline : IPipeline;
        void ReleasePipeline(IPipeline pipeline);
        void Flush();
    }
}