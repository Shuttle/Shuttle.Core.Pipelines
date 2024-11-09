using System;
using System.Collections.Generic;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Pipelines;

public class AddEventBefore : IAddEventBefore
{
    private readonly List<Type> _eventsToExecute;
    private readonly Type _pipelineEvent;

    public AddEventBefore(List<Type> eventsToExecute, Type pipelineEvent)
    {
        _eventsToExecute = Guard.AgainstNull(eventsToExecute);
        _pipelineEvent = Guard.AgainstNull(pipelineEvent);
    }

    public void Add<TPipelineEvent>() where TPipelineEvent : class, new()
    {
        var index = _eventsToExecute.IndexOf(_pipelineEvent);

        _eventsToExecute.Insert(index, typeof(TPipelineEvent));
    }
}