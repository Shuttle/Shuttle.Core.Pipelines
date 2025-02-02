﻿using System;
using System.Collections.Generic;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Pipelines;

public class AddEventAfter : IAddEventAfter
{
    private readonly List<Type> _eventsToExecute;
    private readonly Type _pipelineEvent;
    private readonly IPipelineStage _pipelineStage;

    public AddEventAfter(IPipelineStage pipelineStage, List<Type> eventsToExecute, Type pipelineEvent)
    {
        _pipelineStage = Guard.AgainstNull(pipelineStage);
        _eventsToExecute = Guard.AgainstNull(eventsToExecute);
        _pipelineEvent = Guard.AgainstNull(pipelineEvent);
    }

    public IPipelineStage Add<TPipelineEvent>() where TPipelineEvent : class, new()
    {
        var index = _eventsToExecute.IndexOf(_pipelineEvent);

        _eventsToExecute.Insert(index + 1, typeof(TPipelineEvent));

        return _pipelineStage;
    }
}