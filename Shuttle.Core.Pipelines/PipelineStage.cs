using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Pipelines;

public class PipelineStage : IPipelineStage
{
    protected readonly List<Type> PipelineEvents = new();

    public PipelineStage(string name)
    {
        Name = Guard.AgainstNull(name, nameof(name));
        Events = new ReadOnlyCollection<Type>(PipelineEvents);
    }

    public string Name { get; }

    public IEnumerable<Type> Events { get; }

    public IPipelineStage WithEvent<TEvent>() where TEvent : class
    {
        return WithEvent(typeof(TEvent));
    }

    public IPipelineStage WithEvent(Type eventType)
    {
        Guard.AgainstNull(eventType);

        if (PipelineEvents.Contains(eventType))
        {
            throw new InvalidOperationException(string.Format(Resources.PipelineStageEventAlreadyRegisteredException, Name, eventType.FullName));
        }

        PipelineEvents.Add(eventType);

        return this;
    }

    public IRegisterEventBefore BeforeEvent<TEvent>() where TEvent : class
    {
        var eventName = typeof(TEvent).FullName ?? throw new ApplicationException(string.Format(Reflection.Resources.TypeFullNameNullException, typeof(TEvent).Name));
        var pipelineEvent = PipelineEvents.Find(e => (e.FullName ?? throw new ApplicationException(string.Format(Reflection.Resources.TypeFullNameNullException, typeof(TEvent).Name))).Equals(eventName));

        if (pipelineEvent == null)
        {
            throw new InvalidOperationException(string.Format(Resources.PipelineStageEventNotRegistered, Name, eventName));
        }

        return new RegisterEventBefore(PipelineEvents, pipelineEvent);
    }

    public IRegisterEventAfter AfterEvent<TEvent>() where TEvent : class
    {
        var eventName = typeof(TEvent).FullName;
        var pipelineEvent = PipelineEvents.Find(e => (e.FullName ?? throw new ApplicationException(string.Format(Reflection.Resources.TypeFullNameNullException, typeof(TEvent).Name))).Equals(eventName));

        if (pipelineEvent == null)
        {
            throw new InvalidOperationException(string.Format(Resources.PipelineStageEventNotRegistered, Name, eventName));
        }

        return new RegisterEventAfter(this, PipelineEvents, pipelineEvent);
    }
}