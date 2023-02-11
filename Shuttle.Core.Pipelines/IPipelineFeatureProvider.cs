using System;
using System.Collections.Generic;

namespace Shuttle.Core.Pipelines
{
    public interface IPipelineFeatureProvider
    {
        IEnumerable<Type> FeatureTypes { get; }
    }
}