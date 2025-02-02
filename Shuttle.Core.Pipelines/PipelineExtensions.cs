using Shuttle.Core.Contract;

namespace Shuttle.Core.Pipelines;

public static class PipelineExtensions
{
    public static IPipeline AddObserver<T>(this IPipeline pipeline)
    {
        return Guard.AgainstNull(pipeline).AddObserver(typeof(T));
    }
}