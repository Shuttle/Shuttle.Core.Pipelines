using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Pipelines
{
    public class Pipeline : IPipeline
    {
        private readonly OnAbortPipeline _onAbortPipeline = new OnAbortPipeline();
        private readonly OnExecutionCancelled _onExecutionCancelled = new OnExecutionCancelled();
        private readonly OnPipelineException _onPipelineException = new OnPipelineException();
        private readonly OnPipelineStarting _onPipelineStarting = new OnPipelineStarting();
        private readonly OnStageCompleted _onStageCompleted = new OnStageCompleted();
        private readonly Type _onStageCompletedType = typeof(OnStageCompleted);
        private readonly OnStageStarting _onStageStarting = new OnStageStarting();
        private readonly Type _onStageStartingType = typeof(OnStageStarting);
        private readonly PipelineEventArgs _pipelineEventArgs;

        private readonly Type _pipelineObserverType = typeof(IPipelineObserver<>);
        private readonly string _raisingPipelineEvent = Resources.VerboseRaisingPipelineEvent;

        protected readonly Dictionary<Type, List<ObserverMethodInvoker>> ObservedEvents = new Dictionary<Type, List<ObserverMethodInvoker>>();

        protected readonly List<IPipelineObserver> Observers = new List<IPipelineObserver>();

        protected List<IPipelineStage> Stages = new List<IPipelineStage>();

        private bool _initialized;

        public Pipeline()
        {
            Id = Guid.NewGuid();
            State = new State();

            _onAbortPipeline.Reset(this);
            _onPipelineException.Reset(this);
            _onExecutionCancelled.Reset(this);

            _pipelineEventArgs = new PipelineEventArgs(this);

            var stage = new PipelineStage("__PipelineEntry");

            stage.WithEvent(_onPipelineStarting);

            Stages.Add(stage);
        }

        public event EventHandler<PipelineEventArgs> StageStarting;
        public event EventHandler<PipelineEventArgs> StageCompleted;
        public event EventHandler<PipelineEventArgs> PipelineStarting;
        public event EventHandler<PipelineEventArgs> PipelineCompleted;
        public Guid Id { get; }
        public bool ExceptionHandled { get; internal set; }
        public Exception Exception { get; internal set; }
        public bool Aborted { get; internal set; }
        public string StageName { get; private set; }
        public IPipelineEvent Event { get; private set; }
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
                    ObservedEvents.Add(pipelineEventType, new List<ObserverMethodInvoker>());
                }

                ObservedEvents[pipelineEventType].Add(new ObserverMethodInvoker(pipelineObserver, pipelineEventType));
            }

            return this;
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
                        optimizedStage.WithEvent(@event.Reset(this));
                    }

                    optimizedStages.Add(optimizedStage);
                }
            }

            Stages = optimizedStages;
        }

        public void Abort()
        {
            Aborted = true;
        }

        public void MarkExceptionHandled()
        {
            ExceptionHandled = true;
        }

        public virtual bool Execute(CancellationToken cancellationToken = default)
        {
            return ExecuteAsync(cancellationToken, true).GetAwaiter().GetResult();
        }

        public virtual async Task<bool> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(cancellationToken, false).ConfigureAwait(false);
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

            return result;
        }

        private async Task<bool> ExecuteAsync(CancellationToken cancellationToken, bool sync)
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
                            if (sync)
                            {
                                RaiseEventAsync(_onExecutionCancelled, false, true).GetAwaiter().GetResult();
                            }
                            else
                            {
                                await RaiseEventAsync(_onExecutionCancelled, false, false).ConfigureAwait(false);
                            }

                            return false;
                        }

                        Event = @event;

                        if (sync)
                        {
                            RaiseEventAsync(@event, false, true).GetAwaiter().GetResult();
                        }
                        else
                        {
                            await RaiseEventAsync(@event, false, false).ConfigureAwait(false);
                        }

                        if (!Aborted)
                        {
                            continue;
                        }

                        if (sync)
                        {
                            RaiseEventAsync(_onAbortPipeline, false, true).GetAwaiter().GetResult();
                        }
                        else
                        {
                            await RaiseEventAsync(_onAbortPipeline, false, false).ConfigureAwait(false);
                        }

                        return false;
                    }
                    catch (Exception ex)
                    {
                        Exception = ex.TrimLeading<TargetInvocationException>();

                        ExceptionHandled = false;

                        if (sync)
                        {
                            RaiseEventAsync(_onPipelineException, true, true).GetAwaiter().GetResult();
                        }
                        else
                        {
                            await RaiseEventAsync(_onPipelineException, true, false).ConfigureAwait(false);
                        }

                        if (!ExceptionHandled)
                        {
                            throw;
                        }

                        if (!Aborted)
                        {
                            continue;
                        }

                        if (sync)
                        {
                            RaiseEventAsync(_onAbortPipeline, false, true).GetAwaiter().GetResult();
                        }
                        else
                        {
                            await RaiseEventAsync(_onAbortPipeline, false, false).ConfigureAwait(false);
                        }

                        return false;
                    }
                }

                StageCompleted?.Invoke(this, _pipelineEventArgs);
            }

            PipelineCompleted?.Invoke(this, _pipelineEventArgs);

            return true;
        }

        private async Task RaiseEventAsync(IPipelineEvent @event, bool ignoreAbort, bool sync)
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
                    if (sync)
                    {
                        observer.Invoke(@event);
                    }
                    else
                    {
                        await observer.InvokeAsync(@event).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    throw new PipelineException(string.Format(_raisingPipelineEvent, @event.Name, StageName, observer.GetObserverTypeName()), ex);
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

            private readonly InvokeHandler _invoker;
            private readonly AsyncInvokeHandler _asyncInvoker;

            private delegate void InvokeHandler(IPipelineObserver pipelineObserver, IPipelineEvent @event);

            private delegate Task AsyncInvokeHandler(IPipelineObserver pipelineObserver, IPipelineEvent @event);

            public ObserverMethodInvoker(IPipelineObserver pipelineObserver, Type pipelineEventType)
            {
                _pipelineObserver = pipelineObserver;

                var pipelineObserverType = pipelineObserver.GetType();

                var dynamicMethod = new DynamicMethod(string.Empty,
                    typeof(void), new[] { typeof(IPipelineObserver), typeof(IPipelineEvent) },
                    typeof(IPipelineEvent).Module);

                var methodInfo = pipelineObserverType.GetMethod("Execute", new[] { pipelineEventType });

                var il = dynamicMethod.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.EmitCall(OpCodes.Callvirt, methodInfo, null);
                il.Emit(OpCodes.Ret);

                _invoker = (InvokeHandler)dynamicMethod.CreateDelegate(typeof(InvokeHandler));

                var dynamicMethodAsync = new DynamicMethod(string.Empty, typeof(Task),
                    new[]
                    {
                        typeof(IPipelineObserver),
                        typeof(IPipelineEvent)
                    },
                    typeof(IPipelineEvent).Module);

                var methodInfoAsync = pipelineObserverType.GetMethod("ExecuteAsync", new[] { pipelineEventType });

                var ilAsync = dynamicMethodAsync.GetILGenerator();

                ilAsync.Emit(OpCodes.Ldarg_0);
                ilAsync.Emit(OpCodes.Ldarg_1);
                ilAsync.EmitCall(OpCodes.Callvirt, methodInfoAsync, null);
                ilAsync.Emit(OpCodes.Ret);

                _asyncInvoker = (AsyncInvokeHandler)dynamicMethodAsync.CreateDelegate(typeof(AsyncInvokeHandler));
            }

            public void Invoke(IPipelineEvent @event)
            {
                _invoker.Invoke(_pipelineObserver, @event);
            }

            public async Task InvokeAsync(IPipelineEvent @event)
            {
                await _asyncInvoker.Invoke(_pipelineObserver, @event).ConfigureAwait(false);
            }

            public string GetObserverTypeName()
            {
                return _pipelineObserver.GetType().FullName;
            }
        }
    }
}