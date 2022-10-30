using System;
using System.Collections.Generic;
using System.Linq;

namespace Shuttle.Core.Pipelines
{
    public class ModulesResolvedEventArgs : EventArgs
    {
        public IEnumerable<Type> ModuleTypes { get; }

        public ModulesResolvedEventArgs(IEnumerable<Type> moduleTypes)
        {
            ModuleTypes = moduleTypes ?? Enumerable.Empty<Type>();
        }
    }
}