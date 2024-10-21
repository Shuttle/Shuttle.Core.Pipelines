using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Pipelines;

public class PipelineFactory : IPipelineFactory
{
    private readonly IServiceProvider _serviceProvider;
    private ReusableObjectPool<object> _pool;
    private readonly PipelineOptions _pipelineOptions;

    public PipelineFactory(IOptions<PipelineOptions> pipelineOptions, IServiceProvider serviceProvider)
    {
        _pipelineOptions = Guard.AgainstNull(pipelineOptions).Value;
        _serviceProvider = Guard.AgainstNull(serviceProvider);
        _pool = new();
    }

    public event EventHandler<PipelineEventArgs>? PipelineCreated;
    public event EventHandler<PipelineEventArgs>? PipelineObtained;
    public event EventHandler<PipelineEventArgs>? PipelineReleased;

    public TPipeline GetPipeline<TPipeline>() where TPipeline : IPipeline
    {
        var pipeline = _pool.Get(typeof(TPipeline));

        if (pipeline == null)
        {
            var type = typeof(TPipeline);

            pipeline = (TPipeline)_serviceProvider.GetRequiredService(type);

            if (pipeline == null)
            {
                throw new InvalidOperationException(string.Format(Resources.NullPipelineException, type.FullName));
            }

            if (_pool.Contains(pipeline))
            {
                throw new InvalidOperationException(string.Format(Resources.DuplicatePipelineInstanceException, type.FullName));
            }

            PipelineCreated?.Invoke(this, new((TPipeline)pipeline));
        }
        else
        {
            PipelineObtained?.Invoke(this, new((TPipeline)pipeline));
        }

        return (TPipeline)pipeline;
    }

    public void ReleasePipeline(IPipeline pipeline)
    {
        if (!_pipelineOptions.ReusePipelines)
        {
            return;
        }

        Guard.AgainstNull(pipeline);

        _pool.Release(pipeline);

        PipelineReleased?.Invoke(this, new(pipeline));
    }

    public void Flush()
    {
        _pool.Dispose();

        _pool = new();
    }
}