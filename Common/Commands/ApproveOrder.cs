using Common.Events;
using Newtonsoft.Json;
using System;

namespace Common.Commands
{
    [MessageNamespace("order.approve")]
    public class ApproveOrder : ICommand
    {
        public Guid Id { get; }

        [JsonConstructor]
        public ApproveOrder(Guid id)
        {
            Id = id;
        }
    }
}
