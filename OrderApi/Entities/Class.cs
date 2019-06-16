using System;
using System.Collections.Generic;

namespace OrderApi.Entities
{
    public class Order
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime CreatedOn { get; set; }

        public IDictionary<Guid, int> Products { get; set; }
        public OrderStatus Status { get; set; }

        public Order(Guid id, Guid customerId, IDictionary<Guid, int> products)
        {
            Id = id;
            CustomerId = customerId;
            Products = products;
            CreatedOn = DateTime.Now;
            Status = OrderStatus.Created;
        }
    }

    public enum OrderStatus : byte
    {
        Created = 0,
        Approved = 1,
        Completed = 2,
        Canceled = 3,
        Revoked = 4
    }
}
