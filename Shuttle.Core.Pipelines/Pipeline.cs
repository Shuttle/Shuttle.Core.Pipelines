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
    private readonly OnAbortPipeline _onAbortPipeline;
    private readonly OnExecutionCancelled _onExecutionCancelled;
    private readonly OnPipelineException _onPipelineException;
    private readonly OnStageCompleted _onStageCompleted;
    private readonly OnStageStarting _onStageStarting;

    private readonly Type _onStageCompletedType = typeof(OnStageCompleted);
    private readonly Type _onStageStartingType = typeof(OnStageStarting);
    
    private readonly PipelineEventArgs _pipelineEventArgs;

    private readonly Type _pipelineObserverType = typeof(IPipelineObserver<>);
    private readonly string _raisingPipelineEvent = Resources.VerboseRaisingPipelineEvent;

    protected readonly Dictionary<Type, List<ObserverMethodInvoker>> ObservedEvents = new();

    protected readonly List<IPipelineObserver> Observers = new();

    private bool _initialized;

    protected List<IPipelineStage> Stages = new();

    public Pipeline()
    {
        Id = Guid.NewGuid();
        State = new State();

        _onAbortPipeline = new(this);
        _onPipelineException = new(this);
        _onExecutionCancelled = new(this);
        _onStageCompleted = new(this);
        _onStageStarting = new(this);

        _pipelineEventArgs = new(this);

        var stage = new PipelineStage("__PipelineEntry");

        stage.WithEvent(new OnPipelineStarting(this));

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
    public IPipelineEvent? Event { get; private set; }
    public CancellationToken CancellationToken { get; private set; } = CancellationToken.None;

    public IState State { get; }

    public IPipeline RegisterObserver(IPipelineObserver pipelineObserver)
    {
        Guard.AgainstNull(pipelineObserver, nameof(pipelineObserver));

        Observers.Add(pipelineObserver);

        var observerInterfaces = pipelineObserver.GetType().GetInterfaces();

        var implementedEvents = from i in observerInterfaces
            where
                i.IsGenericType &&
                _pipelineObserverType.GetTypeInfo().IsAssignableFrom(i.GetGenericTypeDefinition()) &&
                i.Name.StartsWith("IPipelineObserver`")
            select i;

        foreach (var @event in implementedEvents)
        {
            var pipelineEventType = @event.GetGenericArguments()[0];

            if (!ObservedEvents.TryGetValue(pipelineEventType, out _))
            {
                ObservedEvents.Add(pipelineEventType, new());
            }

            ObservedEvents[pipelineEventType].Add(new(pipelineObserver, pipelineEventType));
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
        Guard.AgainstNullOrEmptyString(name, nameof(name));

        var stage = new PipelineStage(name);

        Stages.Add(stage);

        return stage;
    }

    public IPipelineStage GetStage(string name)
    {
        Guard.AgainstNullOrEmptyString(name, nameof(name));

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

            foreach (var @event in stage.Events)
            {
                try
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        await RaiseEventAsync(_onExecutionCancelled, false).ConfigureAwait(false);

                        return false;
                    }

                    Event = @event;

                    await RaiseEventAsync(@event, false).ConfigureAwait(false);

                    if (!Aborted)
                    {
                        continue;
                    }

                    await RaiseEventAsync(_onAbortPipeline, false).ConfigureAwait(false);

                    return false;
                }
                catch (Exception ex)
                {
                    Exception = ex.TrimLeading<TargetInvocationException>();

                    ExceptionHandled = false;

                    await RaiseEventAsync(_onPipelineException, true).ConfigureAwait(false);

                    if (!ExceptionHandled)
                    {
                        throw;
                    }

                    if (!Aborted)
                    {
                        continue;
                    }

                    await RaiseEventAsync(_onAbortPipeline, false).ConfigureAwait(false);

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
            var events = new List<IPipelineEvent>();

            if (ObservedEvents.ContainsKey(_onStageStartingType))
            {
                events.Add(_onStageStarting);
            }

            events.AddRange(stage.Events.Where(item => ObservedEvents.ContainsKey(item.GetType())));

            if (ObservedEvents.ContainsKey(_onStageCompletedType))
            {
                events.Add(_onStageCompleted);
            }

            if (events.Any())
            {
                var optimizedStage = new PipelineStage(stage.Name);

                foreach (var @event in events)
                {
                    if (Activator.CreateInstance(@event.GetType(), this) is not IPipelineEvent e)
                    {
                        throw new PipelineException(string.Format(Resources.PipelineEventActivationException, @event.GetType().FullName));
                    }

                    optimizedStage.WithEvent(e);
                }

                optimizedStages.Add(optimizedStage);
            }
        }

        Stages = optimizedStages;
    }

    private async Task RaiseEventAsync(IPipelineEvent @event, bool ignoreAbort)
    {
        ObservedEvents.TryGetValue(@event.GetType(), out var observersForEvent);

        if (observersForEvent == null || observersForEvent.Count == 0)
        {
            return;
        }

        foreach (var observer in observersForEvent)
        {
            try
            {
                await observer.InvokeAsync(@event).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new PipelineException(string.Format(_raisingPipelineEvent, @event.GetType().FullName, StageName, observer.GetObserverTypeName()), ex);
            }

            if (Aborted && !ignoreAbort)
            {
                return;
            }
        }
    }

    protected readonly struct ObserverMethodInvoker
    {
        private readonly IPipelineObserver _pipelineObserver;

        private readonly AsyncInvokeHandler _asyncInvoker;

        private delegate Task AsyncInvokeHandler(IPipelineObserver pipelineObserver, IPipelineEvent @event);

        public ObserverMethodInvoker(IPipelineObserver pipelineObserver, Type pipelineEventType)
        {
            _pipelineObserver = pipelineObserver;

            var pipelineObserverType = pipelineObserver.GetType();

            var dynamicMethodAsync = new DynamicMethod(string.Empty, typeof(Task),
                new[]
                {
                    typeof(IPipelineObserver),
                    typeof(IPipelineEvent)
                },
                typeof(IPipelineEvent).Module);

            var methodInfoAsync = pipelineObserverType.GetMethod("ExecuteAsync", new[] { pipelineEventType });

            if (methodInfoAsync == null)
            {
                throw new PipelineException(string.Format(Resources.ObserverMethodNotFound, pipelineObserverType.FullName));
            }

            var ilAsync = dynamicMethodAsync.GetILGenerator();

            ilAsync.Emit(OpCodes.Ldarg_0);
            ilAsync.Emit(OpCodes.Ldarg_1);
            ilAsync.EmitCall(OpCodes.Callvirt, methodInfoAsync, null);
            ilAsync.Emit(OpCodes.Ret);

            _asyncInvoker = (AsyncInvokeHandler)dynamicMethodAsync.CreateDelegate(typeof(AsyncInvokeHandler));
        }

        public async Task InvokeAsync(IPipelineEvent @event)
        {
            await _asyncInvoker.Invoke(_pipelineObserver, @event).ConfigureAwait(false);
        }

        public string GetObserverTypeName()
        {
            return _pipelineObserver.GetType().FullName ?? throw new ApplicationException(string.Format(Reflection.Resources.TypeFullNameNullException, _pipelineObserver.GetType().Name));
        }
    }
}