﻿using Shuttle.Core.Contract;

namespace Shuttle.Core.Pipelines
{
    public abstract class PipelineEvent : IPipelineEvent
    {
        public IPipeline Pipeline { get; private set; }

        public string Name => GetType().FullName;

        public IPipelineEvent Reset(IPipeline pipeline)
        {
            Pipeline = Guard.AgainstNull(pipeline, nameof(pipeline));

            return this;
        }
    }
}