using Common.Events;
using RabbitMQ.Client.Core.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;

namespace Common
{
    public class BusPublisher : IBusPublisher
    {
        private readonly IQueueService _queueService;

        public BusPublisher(IQueueService queueService)
        {
            _queueService = queueService;
        }

        Task IBusPublisher.PublishAsync<TEvent>(TEvent @event)
        {
            return Send(@event);
        }

        Task IBusPublisher.SendAsync<TCommand>(TCommand command)
        {
            return Send(command);
        }

        private Task Send(IMessage message)
        {
            var attribute = message.GetType()
                    .GetCustomAttributes(typeof(MessageNamespaceAttribute), true)
                    .FirstOrDefault() as MessageNamespaceAttribute;

            return _queueService.SendAsync(
                @object: message,
                exchangeName: "exchange.name",
                routingKey: attribute.Namespace,
                secondsDelay: 10);
        }
    }
}
