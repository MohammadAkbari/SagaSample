using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Common.Commands
{
    public class ReleaseProducts : ICommand
    {
        public Guid OrderId { get; set; }
        public IDictionary<Guid, int> Products { get; }

        [JsonConstructor]
        public ReleaseProducts(Guid orderId, IDictionary<Guid, int> products)
        {
            OrderId = orderId;
            Products = products;
        }
    }
}
