using System.Collections.Generic;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Pipelines
{
    public class RegisterEventAfter : IRegisterEventAfter
    {
        private readonly List<IPipelineEvent> _eventsToExecute;
        private readonly IPipelineEvent _pipelineEvent;
        private readonly IPipelineStage _pipelineStage;

        public RegisterEventAfter(IPipelineStage pipelineStage, List<IPipelineEvent> eventsToExecute,
            IPipelineEvent pipelineEvent)
        {
            _pipelineStage = pipelineStage;
            _eventsToExecute = eventsToExecute;
            _pipelineEvent = pipelineEvent;
        }

        public IPipelineStage Register<TPipelineEvent>() where TPipelineEvent : IPipelineEvent, new()
        {
            return Register(new TPipelineEvent());
        }

        public IPipelineStage Register(IPipelineEvent pipelineEventToRegister)
        {
            Guard.AgainstNull(pipelineEventToRegister, nameof(pipelineEventToRegister));

            var index = _eventsToExecute.IndexOf(_pipelineEvent);

            _eventsToExecute.Insert(index + 1, pipelineEventToRegister);

            return _pipelineStage;
        }
    }
}