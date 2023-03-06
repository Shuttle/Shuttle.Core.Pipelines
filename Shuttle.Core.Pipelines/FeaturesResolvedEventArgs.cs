using System;
using System.Collections.Generic;
using System.Linq;

namespace Shuttle.Core.Pipelines
{
    public class FeaturesResolvedEventArgs : EventArgs
    {
        public IEnumerable<IPipelineFeature> Features { get; }

        public FeaturesResolvedEventArgs(IEnumerable<IPipelineFeature> features)
        {
            Features = features ?? Enumerable.Empty<IPipelineFeature>();
        }
    }
}