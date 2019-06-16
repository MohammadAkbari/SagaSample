using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Events;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrderApi.Entities;
using RabbitMQ.Client.Core.DependencyInjection;

namespace OrderApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IQueueService _queueService;
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<ValuesController> _logger;
        public ValuesController(IQueueService queueService, IDistributedCache distributedCache, ILogger<ValuesController> logger)
        {
            _queueService = queueService;
            _distributedCache = distributedCache;
            _logger = logger;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            _logger.LogInformation("**********************************");

            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post()
        {
            var productId = Guid.NewGuid();
            _distributedCache.SetString(productId.ToString(), 4.ToString());

            var products = new Dictionary<Guid, int>();
            products.Add(productId, 5);

            var order = new Order(Guid.NewGuid(), Guid.NewGuid(), products);

            var v = JsonConvert.SerializeObject(order);

            _distributedCache.SetString("o1", v);

            var orderCreated = new OrderCreated(order.Id, order.CustomerId, order.Products);

            _queueService.Send(
                @object: orderCreated,
                exchangeName: "exchange.name",
                routingKey: "event.order-created",
                secondsDelay: 10);
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
