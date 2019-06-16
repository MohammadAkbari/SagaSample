using Common.Commands;
using Common.Events;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using OrderApi.Entities;
using RabbitMQ.Client.Core.DependencyInjection;
using System.Threading.Tasks;

namespace OrderApi.CommandHandlers
{
    public class RevokeOrderCommandHandler : IAsyncNonCyclicMessageHandler
    {
        private readonly IDistributedCache _distributedCache;

        public RevokeOrderCommandHandler(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public Task Handle(string message, string routingKey, IQueueService queueService)
        {
            var approveOrder = JsonConvert.DeserializeObject<RevokeOrder>(message);

            var v = _distributedCache.GetString("o1");

            var order = JsonConvert.DeserializeObject<Order>(v);

            order.Status = OrderStatus.Revoked;

            var serializedOrder = JsonConvert.SerializeObject(order);

            _distributedCache.SetString("o1", serializedOrder);

            return queueService.SendAsync(
                @object: new OrderRevoked(order.Id, order.CustomerId),
                exchangeName: "exchange.name",
                routingKey: "event.order-revoked",
                secondsDelay: 10);
        }
    }
}
