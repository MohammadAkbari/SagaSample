using Chronicle;
using Common.Events;
using RabbitMQ.Client.Core.DependencyInjection;
using System.Threading.Tasks;

namespace SagaProcessManager.EventHandlers
{
    public class OrderCreatedMessageHandler : BaseMessageHandler, IAsyncMessageHandler
    {
        public OrderCreatedMessageHandler(ISagaCoordinator sagaCoordinator) : base(sagaCoordinator)
        {
        }

        public Task Handle(string message, string routingKey)
        {
            return Handle<OrderCreated>(message, routingKey);
        }
    }
}
