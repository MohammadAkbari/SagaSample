using Chronicle;
using Common.Events;
using RabbitMQ.Client.Core.DependencyInjection;
using System.Threading.Tasks;

namespace SagaProcessManager.EventHandlers
{
    public class ReserveProductsRejectedMessageHandler : BaseMessageHandler, IAsyncMessageHandler
    {
        public ReserveProductsRejectedMessageHandler(ISagaCoordinator sagaCoordinator) : base(sagaCoordinator)
        {
        }

        public Task Handle(string message, string routingKey)
        {
            return Handle<ReserveProductsRejected>(message, routingKey);
        }
    }
}
