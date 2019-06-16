using Common.Commands;
using Common.Events;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client.Core.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProductApi.CommandHandlers
{
    public class ReserveProductsCommandHandler : IAsyncNonCyclicMessageHandler
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<ReserveProductsCommandHandler> _logger;

        public ReserveProductsCommandHandler(IDistributedCache distributedCache, ILogger<ReserveProductsCommandHandler> logger)
        {
            _distributedCache = distributedCache;
            _logger = logger;
        }

        public Task Handle(string message, string routingKey, IQueueService queueService)
        {
            _logger.LogInformation($"ReserveProductsCommandHandler");

            var reserveProducts = JsonConvert.DeserializeObject<ReserveProducts>(message);

            var product = reserveProducts.Products.FirstOrDefault();

            var v = _distributedCache.GetString(product.Key.ToString());
            var count = Convert.ToInt32(v);

            if (product.Value <= count)
            {
                _distributedCache.SetString(product.Key.ToString(), (count - product.Value).ToString());

                return queueService.SendAsync(
                    @object: new ProductsReserved(reserveProducts.OrderId, reserveProducts.Products),
                    exchangeName: "exchange.name",
                    routingKey: "event.products-reserved",
                    secondsDelay: 10);
            }

            return queueService.SendAsync(
                @object: new ReserveProductsRejected(reserveProducts.OrderId, "count is greater than available", ""),
                exchangeName: "exchange.name",
                routingKey: "event.reserve-products-rejected",
                secondsDelay: 10);
        }
    }
}
