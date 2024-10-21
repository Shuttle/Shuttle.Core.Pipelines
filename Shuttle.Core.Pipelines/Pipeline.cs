using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Reflection;

namespace Shuttle.Core.Pipelines;

public class Pipeline : IPipeline
{
    private readonly Type _onAbortPipelineType = typeof(OnAbortPipeline);
    private readonly Type _onExecutionCancelledType = typeof(OnExecutionCancelled);
    private readonly Type _onPipelineExceptionType = typeof(OnPipelineException);
    private readonly Type _onStageCompletedType = typeof(OnStageCompleted);
    private readonly Type _onStageStartingType = typeof(OnStageStarting);
    private readonly Type _pipelineObserverType = typeof(IPipelineObserver<>);

    private readonly PipelineEventArgs _pipelineEventArgs;

    private readonly string _raisingPipelineEvent = Resources.VerboseRaisingPipelineEvent;

    private readonly Dictionary<Type, List<ObserverMethodInvoker>> _observerMethodInvokers = new();
    private readonly Dictionary<Type, ContextConstructorInvoker> _pipelineContextConstructors = new();

    protected readonly List<IPipelineObserver> Observers = new();

    private readonly SemaphoreSlim _lock = new(1, 1);

    private bool _initialized;

    protected List<IPipelineStage> Stages = new();

    public Pipeline()
    {
        Id = Guid.NewGuid();
        State = new State();

        _pipelineEventArgs = new(this);

        var stage = new PipelineStage("__PipelineEntry");

        stage.WithEvent<OnPipelineStarting>();

        Stages.Add(stage);
    }

    public event EventHandler<PipelineEventArgs>? StageStarting;
    public event EventHandler<PipelineEventArgs>? StageCompleted;
    public event EventHandler<PipelineEventArgs>? PipelineStarting;
    public event EventHandler<PipelineEventArgs>? PipelineCompleted;

    public Guid Id { get; }
    public bool ExceptionHandled { get; internal set; }
    public Exception? Exception { get; internal set; }
    public bool Aborted { get; internal set; }
    public string StageName { get; private set; } = "__PipelineEntry";
    public CancellationToken CancellationToken { get; private set; } = CancellationToken.None;

    public IState State { get; }

    public IPipeline RegisterObserver(IPipelineObserver pipelineObserver)
    {
        Observers.Add(Guard.AgainstNull(pipelineObserver));

        var observerInterfaces = pipelineObserver.GetType().GetInterfaces();

        var eventInterfaces = observerInterfaces
            .Where(item => 
                item.IsGenericType && 
                _pipelineObserverType.GetTypeInfo().IsAssignableFrom(item.GetGenericTypeDefinition()) && 
                item.Name.StartsWith("IPipelineObserver`"));

        foreach (var eventInterface in eventInterfaces)
        {
            var pipelineEventType = eventInterface.GetGenericArguments()[0];

            if (!_observerMethodInvokers.TryGetValue(pipelineEventType, out _))
            {
                _observerMethodInvokers.Add(pipelineEventType, new());
            }

            var genericType = _pipelineObserverType.MakeGenericType(pipelineEventType);

            var methodInfo = pipelineObserver.GetType()
                .GetInterfaceMap(genericType)
                .TargetMethods.SingleOrDefault();

            if (methodInfo == null)
            {
                throw new PipelineException(string.Format(Resources.ObserverMethodNotFoundException, pipelineObserver.GetType().FullName, eventInterface.FullName));
            }

            _observerMethodInvokers[pipelineEventType].Add(new(pipelineObserver, methodInfo));
        }

        return this;
    }

    public void Abort()
    {
        Aborted = true;
    }

    public void MarkExceptionHandled()
    {
        ExceptionHandled = true;
    }

    public IPipelineStage RegisterStage(string name)
    {
        var stage = new PipelineStage(Guard.AgainstNullOrEmptyString(name));

        Stages.Add(stage);

        return stage;
    }

    public IPipelineStage GetStage(string name)
    {
        Guard.AgainstNullOrEmptyString(name);

        var result = Stages.Find(stage => stage.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

        Guard.Against<IndexOutOfRangeException>(result == null, string.Format(Resources.PipelineStageNotFound, name));

        return result!;
    }

    public virtual async Task<bool> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (!_initialized)
        {
            Initialize();

            _initialized = true;
        }

        Aborted = false;
        Exception = null;

        CancellationToken = cancellationToken;

        PipelineStarting?.Invoke(this, _pipelineEventArgs);

        foreach (var stage in Stages)
        {
            StageName = stage.Name;

            StageStarting?.Invoke(this, _pipelineEventArgs);

            foreach (var eventType in stage.Events)
            {
                try
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        await RaiseEventAsync(_onExecutionCancelledType, false).ConfigureAwait(false);

                        return false;
                    }

                    await RaiseEventAsync(eventType, false).ConfigureAwait(false);

                    if (!Aborted)
                    {
                        continue;
                    }

                    await RaiseEventAsync(_onAbortPipelineType, false).ConfigureAwait(false);

                    return false;
                }
                catch (Exception ex)
                {
                    Exception = ex.TrimLeading<TargetInvocationException>();

                    ExceptionHandled = false;

                    await RaiseEventAsync(_onPipelineExceptionType, true).ConfigureAwait(false);

                    if (!ExceptionHandled)
                    {
                        throw;
                    }

                    if (!Aborted)
                    {
                        continue;
                    }

                    await RaiseEventAsync(_onAbortPipelineType, false).ConfigureAwait(false);

                    return false;
                }
            }

            StageCompleted?.Invoke(this, _pipelineEventArgs);
        }

        PipelineCompleted?.Invoke(this, _pipelineEventArgs);

        return true;
    }

    private void Initialize()
    {
        var optimizedStages = new List<IPipelineStage>();

        foreach (var stage in Stages)
        {
            var events = new List<Type>();

            if (_observerMethodInvokers.ContainsKey(_onStageStartingType))
            {
                events.Add(_onStageStartingType);
            }

            events.AddRange(stage.Events.Where(item => _observerMethodInvokers.ContainsKey(item)));

            if (_observerMethodInvokers.ContainsKey(_onStageCompletedType))
            {
                events.Add(_onStageCompletedType);
            }

            if (events.Any())
            {
                var optimizedStage = new PipelineStage(stage.Name);

                foreach (var @event in events)
                {
                    optimizedStage.WithEvent(@event);
                }

                optimizedStages.Add(optimizedStage);
            }
        }

        Stages = optimizedStages;
    }

    private async Task RaiseEventAsync(Type eventType, bool ignoreAbort)
    {
        _observerMethodInvokers.TryGetValue(eventType, out var observersForEvent);

        if (observersForEvent == null || observersForEvent.Count == 0)
        {
            return;
        }

        ContextConstructorInvoker? pipelineContextConstructor;

        await _lock.WaitAsync(CancellationToken);

        try
        {
            if (!_pipelineContextConstructors.TryGetValue(eventType, out pipelineContextConstructor))
            {
                pipelineContextConstructor = new(this, eventType);

                _pipelineContextConstructors.Add(eventType, pipelineContextConstructor);
            }
        }
        finally
        {
            _lock.Release();
        }

        var pipelineContext = pipelineContextConstructor.Create();

        foreach (var observer in observersForEvent)
        {
            try
            {
                await observer.InvokeAsync(pipelineContext).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new PipelineException(string.Format(_raisingPipelineEvent, eventType.FullName, StageName, observer.PipelineObserver.GetType().FullName), ex);
            }

            if (Aborted && !ignoreAbort)
            {
                return;
            }
        }
    }

    internal class ContextConstructorInvoker
    {
        private readonly IPipeline _pipeline;
        private readonly Type _pipelineType = typeof(IPipeline);
        private static readonly Type PipelineContext = typeof(PipelineContext<>);

        private readonly ConstructorInvokeHandler _constructorInvoker;

        public ContextConstructorInvoker(IPipeline pipeline, Type eventType)
        {
            _pipeline = pipeline;

            var dynamicMethod = new DynamicMethod(string.Empty, typeof(object),
                new[]
                {
                    typeof(object)
                }, PipelineContext.Module);

            var il = dynamicMethod.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);

            var contextType = PipelineContext.MakeGenericType(eventType);
            var constructorInfo = contextType.GetConstructor(new[]
            {
                _pipelineType
            });

            if (constructorInfo == null)
            {
                throw new InvalidOperationException(string.Format(Resources.ContextConstructorException, contextType.FullName));
            }

            il.Emit(OpCodes.Newobj, constructorInfo);
            il.Emit(OpCodes.Ret);

            _constructorInvoker = (ConstructorInvokeHandler)dynamicMethod.CreateDelegate(typeof(ConstructorInvokeHandler));
        }

        public object Create()
        {
            return _constructorInvoker(_pipeline);
        }

        private delegate object ConstructorInvokeHandler(IPipeline pipeline);
    }

    internal readonly struct ObserverMethodInvoker
    {
        public IPipelineObserver PipelineObserver { get; }

        private static readonly Type PipelineContextType = typeof(PipelineContext<>);

        private readonly AsyncInvokeHandler _asyncInvoker;

        public ObserverMethodInvoker(IPipelineObserver pipelineObserver, MethodInfo methodInfo)
        {
            PipelineObserver = Guard.AgainstNull(pipelineObserver);

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
            await _asyncInvoker.Invoke(PipelineObserver, pipelineContext);
        }

        private delegate Task AsyncInvokeHandler(object observer, object pipelineContext);
    }
}