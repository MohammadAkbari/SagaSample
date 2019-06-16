using Chronicle;
using Common.Events;
using RabbitMQ.Client.Core.DependencyInjection;
using System.Threading.Tasks;

namespace SagaProcessManager.EventHandlers
{
    public class ProductsReservedMessageHandler : BaseMessageHandler, IAsyncMessageHandler
    {
        public ProductsReservedMessageHandler(ISagaCoordinator sagaCoordinator) : base(sagaCoordinator)
        {
        }

        public Task Handle(string message, string routingKey)
        {
            return Handle<ProductsReserved>(message, routingKey);
        }
    }
}
