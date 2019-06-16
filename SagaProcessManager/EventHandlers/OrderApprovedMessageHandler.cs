using Chronicle;
using Common.Events;
using RabbitMQ.Client.Core.DependencyInjection;
using System.Threading.Tasks;

namespace SagaProcessManager.EventHandlers
{
    public class OrderApprovedMessageHandler : BaseMessageHandler, IAsyncMessageHandler
    {
        public OrderApprovedMessageHandler(ISagaCoordinator sagaCoordinator) : base(sagaCoordinator)
        {
        }

        public Task Handle(string message, string routingKey)
        {
            return Handle<OrderApproved>(message, routingKey);
        }
    }
}
