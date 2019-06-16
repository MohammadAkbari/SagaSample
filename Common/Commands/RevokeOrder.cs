using Common.Events;
using Newtonsoft.Json;
using System;

namespace Common.Commands
{
    [MessageNamespace("order.revoke")]
    public class RevokeOrder : ICommand
    {
        public Guid Id { get; }
        public Guid CustomerId { get; }

        [JsonConstructor]
        public RevokeOrder(Guid id, Guid customerId)
        {
            Id = id;
            CustomerId = customerId;
        }
    }
}
