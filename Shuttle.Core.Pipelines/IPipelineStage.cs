using System;
using System.Collections.Generic;

namespace Shuttle.Core.Pipelines;

public interface IPipelineStage
{
    IEnumerable<Type> Events { get; }
    string Name { get; }
    IRegisterEventAfter AfterEvent<TEvent>() where TEvent : class;
    IRegisterEventBefore BeforeEvent<TEvent>() where TEvent : class;
    IPipelineStage WithEvent<TEvent>() where TEvent : class;
}
    