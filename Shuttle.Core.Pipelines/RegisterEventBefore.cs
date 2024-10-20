using System.Collections.Generic;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Pipelines;

public class RegisterEventBefore : IRegisterEventBefore
{
    private readonly List<IPipelineEvent> _eventsToExecute;
    private readonly IPipelineEvent _pipelineEvent;

    public RegisterEventBefore(List<IPipelineEvent> eventsToExecute, IPipelineEvent pipelineEvent)
    {
        _eventsToExecute = Guard.AgainstNull(eventsToExecute);
        _pipelineEvent = Guard.AgainstNull(pipelineEvent);
    }

    public void Register<TPipelineEvent>() where TPipelineEvent : IPipelineEvent, new()
    {
        Register(new TPipelineEvent());
    }

    public void Register(IPipelineEvent pipelineEventToRegister)
    {
        Guard.AgainstNull(pipelineEventToRegister);

        var index = _eventsToExecute.IndexOf(_pipelineEvent);

        _eventsToExecute.Insert(index, pipelineEventToRegister);
    }
}