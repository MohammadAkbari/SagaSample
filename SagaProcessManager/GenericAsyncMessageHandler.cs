using Chronicle;
using Common;
using RabbitMQ.Client.Core.DependencyInjection;
using SagaProcessManager.EventHandlers;
using System.Threading.Tasks;

namespace SagaProcessManager
{
    public class GenericAsyncMessageHandler<T> : BaseMessageHandler, IAsyncMessageHandler where T:class, IMessage
    {
        public GenericAsyncMessageHandler(ISagaCoordinator sagaCoordinator) : base(sagaCoordinator)
        {
        }

        public Task Handle(string message, string routingKey)
        {
            return Handle<T>(message, routingKey);
        }
    }
}
