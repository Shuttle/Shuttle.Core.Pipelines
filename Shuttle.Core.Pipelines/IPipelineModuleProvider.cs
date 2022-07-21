using System;
using System.Collections.Generic;

namespace Shuttle.Core.Pipelines
{
    public interface IPipelineModuleProvider
    {
        IEnumerable<Type> ModuleTypes { get; }
    }
}