using Shuttle.Core.Contract;

namespace Shuttle.Core.Pipelines;

public static class PipelineExtensions
{
    public static IPipeline RegisterObserver<T>(this IPipeline pipeline)
    {
        return Guard.AgainstNull(pipeline).RegisterObserver(typeof(T));
    }
}