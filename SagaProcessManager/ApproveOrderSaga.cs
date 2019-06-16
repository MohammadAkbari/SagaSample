using Chronicle;
using Common.Commands;
using Common.Events;
using RabbitMQ.Client.Core.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace SagaProcessManager
{
    public class ApproveOrderSaga : Saga,
        ISagaStartAction<OrderCreated>,
        ISagaAction<OrderRevoked>,
        ISagaAction<RevokeOrderRejected>,
        ISagaAction<ProductsReserved>,
        ISagaAction<ReserveProductsRejected>,
        ISagaAction<OrderApproved>,
        ISagaAction<ApproveOrderRejected>
    {
        private readonly IQueueService _queueService;

        public ApproveOrderSaga(IQueueService queueService)
            => _queueService = queueService;

        public override Guid ResolveId(object message, ISagaContext context)
        {
            switch (message)
            {
                case OrderCreated m: return m.Id;
                case ProductsReserved m: return m.OrderId;
                case ReserveProductsRejected m: return m.OrderId;
                case OrderApproved m: return m.Id;
                case ApproveOrderRejected m: return m.Id;
                default: return base.ResolveId(message, context);
            }
        }

        public async Task HandleAsync(OrderCreated message, ISagaContext context)
        {
            await _queueService.SendAsync(
                @object: new ReserveProducts(message.Id, message.Products),
                exchangeName: "exchange.name",
                routingKey: "products.reserve",
                secondsDelay: 10);
        }

        public async Task CompensateAsync(OrderCreated message, ISagaContext context)
        {
            await _queueService.SendAsync(
                @object: new RevokeOrder(message.Id, message.CustomerId),
                exchangeName: "exchange.name",
                routingKey: "order.revoke",
                secondsDelay: 10);
        }

        public async Task HandleAsync(ProductsReserved message, ISagaContext context)
        {
            await _queueService.SendAsync(
                @object: new ApproveOrder(message.OrderId),
                exchangeName: "exchange.name",
                routingKey: "order.approve",
                secondsDelay: 10);
        }

        public async Task CompensateAsync(ProductsReserved message, ISagaContext context)
        {
            await _queueService.SendAsync(
                @object: new ReleaseProducts(message.OrderId, message.Products),
                exchangeName: "exchange.name",
                routingKey: "products.release",
                secondsDelay: 10);
        }

        public async Task HandleAsync(ReserveProductsRejected message, ISagaContext context)
        {
            Reject();
            await Task.CompletedTask;
        }

        public async Task CompensateAsync(ReserveProductsRejected message, ISagaContext context)
        {
            await Task.CompletedTask;
        }

        public async Task HandleAsync(OrderApproved message, ISagaContext context)
        {
            Complete();
            await Task.CompletedTask;
        }

        public async Task CompensateAsync(OrderApproved message, ISagaContext context)
        {
            await Task.CompletedTask;
        }

        public async Task HandleAsync(ApproveOrderRejected message, ISagaContext context)
        {
            Reject();
            await Task.CompletedTask;
        }

        public async Task CompensateAsync(ApproveOrderRejected message, ISagaContext context)
        {
            await Task.CompletedTask;
        }

        public async Task HandleAsync(OrderRevoked message, ISagaContext context)
        {
            Complete();
            await Task.CompletedTask;
        }

        public async Task CompensateAsync(OrderRevoked message, ISagaContext context)
        {
            await Task.CompletedTask;
        }

        //Edge case
        public async Task HandleAsync(RevokeOrderRejected message, ISagaContext context)
        {
            Reject();
            await Task.CompletedTask;
        }

        public async Task CompensateAsync(RevokeOrderRejected message, ISagaContext context)
        {
            await Task.CompletedTask;
        }
    }
}
