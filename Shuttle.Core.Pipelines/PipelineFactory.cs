using System;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Pipelines
{
    public class PipelineFactory : IPipelineFactory
    {
        private ReusableObjectPool<object> _pool;
        private readonly IServiceProvider _serviceProvider;

        public PipelineFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = Guard.AgainstNull(serviceProvider, nameof(serviceProvider));
            _pool = new ReusableObjectPool<object>();
        }

        public event EventHandler<PipelineEventArgs> PipelineCreated;

        public event EventHandler<PipelineEventArgs> PipelineObtained;

        public event EventHandler<PipelineEventArgs> PipelineReleased;

        public TPipeline GetPipeline<TPipeline>() where TPipeline : IPipeline
        {
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

                pipeline.Optimize();

                PipelineCreated?.Invoke(this, new PipelineEventArgs(pipeline));
            }
            else
            {
                PipelineObtained?.Invoke(this, new PipelineEventArgs(pipeline));
            }

            return pipeline;
        }

        public void ReleasePipeline(IPipeline pipeline)
        {
            Guard.AgainstNull(pipeline, nameof(pipeline));

            _pool.Release(pipeline);

            PipelineReleased?.Invoke(this, new PipelineEventArgs(pipeline));
        }

        public void Flush()
        {
            _pool.Dispose();

            _pool = new ReusableObjectPool<object>();
        }
    }
}