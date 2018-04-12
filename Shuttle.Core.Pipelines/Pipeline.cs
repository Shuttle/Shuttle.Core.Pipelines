using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Shuttle.Core.Contract;
using Shuttle.Core.Logging;
using Shuttle.Core.Reflection;

namespace Shuttle.Core.Pipelines
{
    public class Pipeline : IPipeline
    {
        private readonly string _enteringPipelineStage = Resources.EnteringPipelineStage;

        private readonly string _executingPipeline = Resources.ExecutingPipeline;

        private readonly string _firstChanceExceptionHandledByPipeline =
            Resources.FirstChanceExceptionHandledByPipeline;

        private readonly ILog _log;
        private readonly OnAbortPipeline _onAbortPipeline = new OnAbortPipeline();
        private readonly OnPipelineException _onPipelineException = new OnPipelineException();

        private readonly OnPipelineStarting _onPipelineStarting = new OnPipelineStarting();
        private readonly string _raisingPipelineEvent = Resources.VerboseRaisingPipelineEvent;

        protected readonly Dictionary<Type, List<ObserverMethodInfoPair>> ObservedEvents =
            new Dictionary<Type, List<ObserverMethodInfoPair>>();

        protected readonly List<IPipelineObserver> Observers = new List<IPipelineObserver>();
        protected readonly List<IPipelineStage> Stages = new List<IPipelineStage>();

        public Pipeline()
        {
            Id = Guid.NewGuid();
            State = new State();
            _onAbortPipeline.Reset(this);
            _onPipelineException.Reset(this);

            var stage = new PipelineStage("__PipelineEntry");

            stage.WithEvent(_onPipelineStarting);

            Stages.Add(stage);

            _log = Log.For(this);
        }

        public Guid Id { get; }
        public bool ExceptionHandled { get; internal set; }
        public Exception Exception { get; internal set; }
        public bool Aborted { get; internal set; }
        public string StageName { get; private set; }
        public IPipelineEvent Event { get; private set; }

        public IState State { get; }

        public IPipeline RegisterObserver(IPipelineObserver pipelineObserver)
        {
            Observers.Add(pipelineObserver);
            var observerInterfaces = pipelineObserver.GetType().GetInterfaces();

            var implementedEvents = from i in observerInterfaces
                where 
                    i.IsAssignableTo(typeof(IPipelineObserver<>))
                    &&
                    i.IsGenericType
                    &&
                    i.Name.StartsWith("IPipelineObserver`")
                select i;

            foreach (var @event in implementedEvents)
            {
                var pipelineEventType = @event.GetGenericArguments()[0];

                if (!ObservedEvents.TryGetValue(pipelineEventType, out _))
                {
                    ObservedEvents.Add(pipelineEventType, new List<ObserverMethodInfoPair>());
                }

                var methodInfo = pipelineObserver.GetType().GetMethod("Execute", new[] {pipelineEventType});
                ObservedEvents[pipelineEventType].Add(new ObserverMethodInfoPair(pipelineObserver, methodInfo));
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

        public virtual bool Execute()
        {
            var result = true;

            Aborted = false;
            ExceptionHandled = false;
            Exception = null;

            if (_log.IsVerboseEnabled)
            {
                _log.Verbose(string.Format(_executingPipeline, GetType().FullName));
            }

            foreach (var stage in Stages)
            {
                StageName = stage.Name;

                if (_log.IsVerboseEnabled)
                {
                    _log.Verbose(string.Format(_enteringPipelineStage, StageName));
                }

                foreach (var @event in stage.Events)
                {
                    try
                    {
                        Event = @event;

                        RaiseEvent(@event.Reset(this));

                        if (Aborted)
                        {
                            result = false;

                            RaiseEvent(_onAbortPipeline);

                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        result = false;

                        Exception = ex.TrimLeading<TargetInvocationException>();

                        RaiseEvent(_onPipelineException, true);

                        if (!ExceptionHandled)
                        {
                            _log.Fatal(string.Format(Resources.UnhandledPipelineException, @event.Name,
                                ex.AllMessages()));

                            throw;
                        }

                        if (_log.IsVerboseEnabled)
                        {
                            _log.Verbose(string.Format(_firstChanceExceptionHandledByPipeline, ex.Message));
                        }

                        if (Aborted)
                        {
                            RaiseEvent(_onAbortPipeline);

                            break;
                        }
                    }
                }

                if (Aborted)
                {
                    break;
                }
            }

            return result;
        }

        public IPipelineStage RegisterStage(string name)
        {
            var stage = new PipelineStage(name);

            Stages.Add(stage);

            return stage;
        }

        public IPipelineStage GetStage(string name)
        {
            var result = Stages.Find(stage => stage.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

            Guard.Against<IndexOutOfRangeException>(result == null,
                string.Format(Resources.PipelineStageNotFound, name));

            return result;
        }

        private void RaiseEvent(IPipelineEvent @event, bool ignoreAbort = false)
        {
            ObservedEvents.TryGetValue(@event.GetType(), out var observersForEvent);

            if (observersForEvent == null || observersForEvent.Count == 0)
            {
                return;
            }

            foreach (var observerPair in observersForEvent)
            {
                var observer = observerPair.PipelineObserver;
                if (_log.IsVerboseEnabled)
                {
                    _log.Verbose(string.Format(_raisingPipelineEvent, @event.Name, StageName,
                        observer.GetType().FullName));
                }

                try
                {
                    observerPair.MethodInfo.Invoke(observer, new object[] {@event});
                }
                catch (Exception ex)
                {
                    throw new PipelineException(
                        string.Format(_raisingPipelineEvent, @event.Name, StageName, observer.GetType().FullName), ex);
                }
                if (Aborted && !ignoreAbort)
                {
                    return;
                }
            }
        }

        protected struct ObserverMethodInfoPair
        {
            public IPipelineObserver PipelineObserver { get; }

            public MethodInfo MethodInfo { get; }

            public ObserverMethodInfoPair(IPipelineObserver pipelineObserver, MethodInfo methodInfo)
            {
                PipelineObserver = pipelineObserver;
                MethodInfo = methodInfo;
            }
        }
    }
}