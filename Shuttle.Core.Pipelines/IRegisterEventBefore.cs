using System;

namespace Shuttle.Core.Pipelines;

public interface IAddEventBefore
{
    void Add<TPipelineEvent>() where TPipelineEvent : class, new();
}