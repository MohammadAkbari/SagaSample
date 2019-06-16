using Chronicle;
using Common.Events;
using RabbitMQ.Client.Core.DependencyInjection;
using System.Threading.Tasks;

namespace SagaProcessManager.EventHandlers
{
    public class RevokeOrderRejectedMessageHandler : BaseMessageHandler, IAsyncMessageHandler
    {
        public RevokeOrderRejectedMessageHandler(ISagaCoordinator sagaCoordinator) : base(sagaCoordinator)
        {
        }

        public Task Handle(string message, string routingKey)
        {
            return Handle<RevokeOrderRejected>(message, routingKey);
        }
    }
}
