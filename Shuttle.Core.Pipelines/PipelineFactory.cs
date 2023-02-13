using System;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Pipelines
{
    public class PipelineFactory : IPipelineFactory
    {
        private ReusableObjectPool<object> _pool;
        private readonly IServiceProvider _serviceProvider;
        private readonly IPipelineFeatureProvider _pipelineFeatureProvider;
        private static readonly object Lock = new object();
        private static volatile bool _featuresResolved = false;

        public PipelineFactory(IServiceProvider serviceProvider, IPipelineFeatureProvider pipelineFeatureProvider)
        {
            _serviceProvider = Guard.AgainstNull(serviceProvider, nameof(serviceProvider));
            _pipelineFeatureProvider = Guard.AgainstNull(pipelineFeatureProvider, nameof(pipelineFeatureProvider));
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

        public event EventHandler<FeaturesResolvedEventArgs> FeaturesResolved = delegate
        {
        };

        public TPipeline GetPipeline<TPipeline>() where TPipeline : IPipeline
        {
            lock (Lock)
            {
                if (!_featuresResolved)
                {
                    foreach (var moduleType in _pipelineFeatureProvider.FeatureTypes)
                    {
                        _serviceProvider.GetRequiredService(moduleType);
                    }

                    _featuresResolved = true;

                    FeaturesResolved.Invoke(this, new FeaturesResolvedEventArgs(_pipelineFeatureProvider.FeatureTypes));
                }
            }

            var pipeline = (TPipeline)_pool.Get(typeof(TPipeline));

            if (pipeline == null)
            {
                var type = typeof(TPipeline);

                pipeline = (TPipeline)_serviceProvider.GetRequiredService(type);

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

        public void Flush()
        {
            _pool.Dispose();

            _pool = new ReusableObjectPool<object>();
        }
    }
}