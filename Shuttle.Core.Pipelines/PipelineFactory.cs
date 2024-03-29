using System;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Pipelines
{
    public class PipelineFactory : IPipelineFactory
    {
        private ReusableObjectPool<object> _pool;
        private readonly IServiceProvider _serviceProvider;
        private readonly IPipelineModuleProvider _pipelineModuleProvider;
        private static readonly object Lock = new object();
        private static volatile bool _modulesResolved = false;

        public PipelineFactory(IServiceProvider serviceProvider, IPipelineModuleProvider pipelineModuleProvider)
        {
            _serviceProvider = Guard.AgainstNull(serviceProvider, nameof(serviceProvider));
            _pipelineModuleProvider = Guard.AgainstNull(pipelineModuleProvider, nameof(pipelineModuleProvider));
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

        public event EventHandler<ModulesResolvedEventArgs> ModulesResolved = delegate
        {
        };

        public TPipeline GetPipeline<TPipeline>() where TPipeline : IPipeline
        {
            lock (Lock)
            {
                if (!_modulesResolved)
                {
                    foreach (var moduleType in _pipelineModuleProvider.ModuleTypes)
                    {
                        _serviceProvider.GetRequiredService(moduleType);
                    }

                    _modulesResolved = true;

                    ModulesResolved.Invoke(this, new ModulesResolvedEventArgs(_pipelineModuleProvider.ModuleTypes));
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