using System;
using System.Collections.Generic;
using System.Linq;

namespace Shuttle.Core.Pipelines
{
    public class PipelineFeatureProvider : IPipelineFeatureProvider
    {
        public IEnumerable<Type> FeatureTypes { get; }

        public PipelineFeatureProvider(IEnumerable<Type> moduleTypes)
        {
            FeatureTypes = moduleTypes ?? Enumerable.Empty<Type>();
        }
    }
}