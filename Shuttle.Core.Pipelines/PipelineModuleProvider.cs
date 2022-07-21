using System;
using System.Collections.Generic;
using System.Linq;

namespace Shuttle.Core.Pipelines
{
    public class PipelineModuleProvider : IPipelineModuleProvider
    {
        public IEnumerable<Type> ModuleTypes { get; }

        public PipelineModuleProvider(IEnumerable<Type> moduleTypes)
        {
            ModuleTypes = moduleTypes ?? Enumerable.Empty<Type>();
        }
    }
}