using System;

namespace Shuttle.Core.Pipelines;

public interface IRegisterEventBefore
{
    void Register<TPipelineEvent>() where TPipelineEvent : class, new();
}