using Chronicle;
using Common.Events;
using RabbitMQ.Client.Core.DependencyInjection;
using System.Threading.Tasks;

namespace SagaProcessManager.EventHandlers
{
    public class ApproveOrderRejectedMessageHandler : BaseMessageHandler, IAsyncMessageHandler
    {
        public ApproveOrderRejectedMessageHandler(ISagaCoordinator sagaCoordinator) : base(sagaCoordinator)
        {
        }

        public Task Handle(string message, string routingKey)
        {
            return Handle<ApproveOrderRejected>(message, routingKey);
        }
    }
}
