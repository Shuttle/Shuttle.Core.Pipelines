using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Pipelines;

internal readonly struct PipelineObserverMethodInvoker
{
    public IPipelineObserverProvider PipelineObserverProvider { get; }

    private static readonly Type PipelineContextType = typeof(PipelineContext<>);

    private readonly AsyncInvokeHandler _asyncInvoker;

    public PipelineObserverMethodInvoker(IPipelineObserverProvider pipelineObserverProvider, MethodInfo methodInfo)
    {
        PipelineObserverProvider = Guard.AgainstNull(pipelineObserverProvider);

        var dynamicMethod = new DynamicMethod(string.Empty, typeof(Task), new[] { typeof(object), typeof(object) }, PipelineContextType.Module);

        var il = dynamicMethod.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_1);

        il.EmitCall(OpCodes.Callvirt, methodInfo, null);
        il.Emit(OpCodes.Ret);

        _asyncInvoker = (AsyncInvokeHandler)dynamicMethod.CreateDelegate(typeof(AsyncInvokeHandler));
    }

    public async Task InvokeAsync(object pipelineContext)
    {
        await _asyncInvoker.Invoke(PipelineObserverProvider.GetObserverInstance(), pipelineContext);
    }

    private delegate Task AsyncInvokeHandler(object observer, object pipelineContext);
}