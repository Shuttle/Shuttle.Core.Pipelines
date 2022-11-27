using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Pipelines
{
    public class PipelineStage : IPipelineStage
    {
        protected readonly List<IPipelineEvent> PipelineEvents = new List<IPipelineEvent>();

        public PipelineStage(string name)
        {
            Name = Guard.AgainstNull(name, nameof(name));
            Events = new ReadOnlyCollection<IPipelineEvent>(PipelineEvents);
        }

        public string Name { get; }

        public IEnumerable<IPipelineEvent> Events { get; }

        public IPipelineStage WithEvent<TPipelineEvent>() where TPipelineEvent : IPipelineEvent, new()
        {
            return WithEvent(new TPipelineEvent());
        }

        public IPipelineStage WithEvent(IPipelineEvent pipelineEvent)
        {
            PipelineEvents.Add(Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent)));

            return this;
        }

        public IRegisterEventBefore BeforeEvent<TPipelineEvent>() where TPipelineEvent : IPipelineEvent, new()
        {
            var eventName = typeof(TPipelineEvent).FullName;
            var pipelineEvent = PipelineEvents.Find(e => e.Name.Equals(eventName));

            if (pipelineEvent == null)
            {
                throw new InvalidOperationException(
                    string.Format(Resources.PipelineStageEventNotRegistered,
                        Name, eventName));
            }

            return new RegisterEventBefore(PipelineEvents, pipelineEvent);
        }

        public IRegisterEventAfter AfterEvent<TPipelineEvent>() where TPipelineEvent : IPipelineEvent, new()
        {
            var eventName = typeof(TPipelineEvent).FullName;
            var pipelineEvent = PipelineEvents.Find(e => e.Name.Equals(eventName));

            if (pipelineEvent == null)
            {
                throw new InvalidOperationException(
                    string.Format(Resources.PipelineStageEventNotRegistered,
                        Name, eventName));
            }

            return new RegisterEventAfter(this, PipelineEvents, pipelineEvent);
        }
    }
}