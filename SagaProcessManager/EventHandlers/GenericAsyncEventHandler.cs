using System;
using System.Threading.Tasks;
using Chronicle;
using Common;
using Newtonsoft.Json;
using RabbitMQ.Client.Core.DependencyInjection;

namespace SagaProcessManager.EventHandlers
{
    public class GenericAsyncEventHandler<T> : IAsyncMessageHandler where T:class, IEvent
    {
        private readonly ISagaCoordinator _sagaCoordinator;
        
        public GenericAsyncEventHandler(ISagaCoordinator sagaCoordinator)
        {
            _sagaCoordinator = sagaCoordinator;
        }

        public Task Handle(string message, string routingKey)
        {
            return Handle(message);
        }

        private Task Handle(string message)
        {
            var context = SagaContext
                .Create()
                .WithCorrelationId(Guid.NewGuid())
                .WithOriginator("Test")
                .WithMetadata("key", "lulz")
                .Build();

            var note = JsonConvert.DeserializeObject<T>(message);

            return !note.BelongsToSaga() ? Task.CompletedTask : _sagaCoordinator.ProcessAsync(note, context);
        }
    }
}
