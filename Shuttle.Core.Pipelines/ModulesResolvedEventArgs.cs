using System;
using System.Collections.Generic;
using System.Linq;

namespace Shuttle.Core.Pipelines
{
    public class FeaturesResolvedEventArgs : EventArgs
    {
        public IEnumerable<Type> FeatureTypes { get; }

        public FeaturesResolvedEventArgs(IEnumerable<Type> featureTypes)
        {
            FeatureTypes = featureTypes ?? Enumerable.Empty<Type>();
        }
    }
}