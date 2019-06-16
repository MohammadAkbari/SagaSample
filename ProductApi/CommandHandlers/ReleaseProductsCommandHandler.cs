using Common.Commands;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client.Core.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProductApi.CommandHandlers
{
    public class ReleaseProductsCommandHandler : IAsyncNonCyclicMessageHandler
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<ReleaseProductsCommandHandler> _logger;
        public ReleaseProductsCommandHandler(IDistributedCache distributedCache, ILogger<ReleaseProductsCommandHandler> logger)
        {
            _distributedCache = distributedCache;
            _logger = logger;
        }

        public Task Handle(string message, string routingKey, IQueueService queueService)
        {
            _logger.LogInformation($"ReleaseProductsCommandHandler");

            var releaseProducts = JsonConvert.DeserializeObject<ReleaseProducts>(message);

            var product = releaseProducts.Products.FirstOrDefault();

            var v = _distributedCache.GetString(product.Key.ToString());
            var count = Convert.ToInt32(v);

            _distributedCache.SetString(product.Key.ToString(), (count + product.Value).ToString());

            return Task.CompletedTask;
        }
    }

}
