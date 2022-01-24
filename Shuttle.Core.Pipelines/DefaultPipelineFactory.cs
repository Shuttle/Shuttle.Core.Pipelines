using System;
using Shuttle.Core.Container;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Pipelines
{
    public class DefaultPipelineFactory : IPipelineFactory
    {
        private readonly ReusableObjectPool<object> _pool;
        private readonly IComponentResolver _resolver;

        public DefaultPipelineFactory()
        {
            _pool = new ReusableObjectPool<object>();
        }

        public DefaultPipelineFactory(IComponentResolver resolver)
        {
            Guard.AgainstNull(resolver, nameof(resolver));

            _resolver = resolver;
        }

        public void OnPipelineCreated(object sender, PipelineEventArgs args)
        {
            PipelineCreated.Invoke(sender, args);
        }

        public void OnPipelineObtained(object sender, PipelineEventArgs args)
        {
            PipelineObtained.Invoke(sender, args);
        }

        public void OnPipelineReleased(object sender, PipelineEventArgs args)
        {
            PipelineReleased.Invoke(sender, args);
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

                pipeline = (TPipeline)_resolver.Resolve(type);

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

                OnPipelineCreated(this, new PipelineEventArgs(pipeline));
            }
            else
            {
                OnPipelineObtained(this, new PipelineEventArgs(pipeline));
            }

            return pipeline;
        }

        public void ReleasePipeline(IPipeline pipeline)
        {
            Guard.AgainstNull(pipeline, nameof(pipeline));

            _pool.Release(pipeline);

            OnPipelineReleased(this, new PipelineEventArgs(pipeline));
        }
    }
}