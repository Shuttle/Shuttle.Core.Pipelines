using System;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Pipelines
{
    public class PipelineFactory : IPipelineFactory
    {
        private readonly ReusableObjectPool<object> _pool;
        private readonly IServiceProvider _provider;

        public PipelineFactory(IServiceProvider provider)
        {
            Guard.AgainstNull(provider, nameof(provider));

            _provider = provider;
            _pool = new ReusableObjectPool<object>();
        }

        public event EventHandler<PipelineEventArgs> PipelineCreated = delegate
        {
        };

        public event EventHandler<PipelineEventArgs> PipelineObtained = delegate
        {
        };

        public event EventHandler<PipelineEventArgs> PipelineReleased = delegate
        {
        };

        public TPipeline GetPipeline<TPipeline>() where TPipeline : IPipeline
        {
            var pipeline = (TPipeline)_pool.Get(typeof(TPipeline));

            if (pipeline == null)
            {
                var type = typeof(TPipeline);

                pipeline = (TPipeline)_provider.GetRequiredService(type);

                if (pipeline == null)
                {
                    throw new InvalidOperationException(
                        string.Format(Resources.NullPipelineException, type.FullName));
                }

                if (_pool.Contains(pipeline))
                {
                    throw new InvalidOperationException(
                        string.Format(Resources.DuplicatePipelineInstanceException, type.FullName));
                }

                PipelineCreated.Invoke(this, new PipelineEventArgs(pipeline));
            }
            else
            {
                PipelineObtained.Invoke(this, new PipelineEventArgs(pipeline));
            }

            return pipeline;
        }

        public void ReleasePipeline(IPipeline pipeline)
        {
            Guard.AgainstNull(pipeline, nameof(pipeline));

            _pool.Release(pipeline);

            PipelineReleased.Invoke(this, new PipelineEventArgs(pipeline));
        }
    }
}