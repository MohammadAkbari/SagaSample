using Chronicle;
using Common.Events;
using RabbitMQ.Client.Core.DependencyInjection;
using System.Threading.Tasks;

namespace SagaProcessManager.EventHandlers
{
    public class OrderRevokedMessageHandler : BaseMessageHandler, IAsyncMessageHandler
    {
        public OrderRevokedMessageHandler(ISagaCoordinator sagaCoordinator) : base(sagaCoordinator)
        {
        }

        public Task Handle(string message, string routingKey)
        {
            return Handle<OrderRevoked>(message, routingKey);
        }
    }
}
