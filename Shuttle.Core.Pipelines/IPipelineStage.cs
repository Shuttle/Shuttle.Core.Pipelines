using System.Collections.Generic;

namespace Shuttle.Core.Pipelines;

public interface IPipelineStage
{
    IEnumerable<PipelineEvent> Events { get; }
    string Name { get; }
    IRegisterEventAfter AfterEvent<TPipelineEvent>() where TPipelineEvent : PipelineEvent;
    IRegisterEventBefore BeforeEvent<TPipelineEvent>() where TPipelineEvent : PipelineEvent;
    IPipelineStage WithEvent<TPipelineEvent>() where TPipelineEvent : PipelineEvent;
    IPipelineStage WithEvent(PipelineEvent pipelineEvent);
}