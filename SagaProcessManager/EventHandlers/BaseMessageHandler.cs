using Chronicle;
using Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SagaProcessManager.EventHandlers
{
    public class BaseMessageHandler
    {
        private readonly ISagaCoordinator _sagaCoordinator;
        public BaseMessageHandler(ISagaCoordinator sagaCoordinator)
        {
            _sagaCoordinator = sagaCoordinator;
        }

        public Task Handle<T>(string message, string routingKey) where T : class, IMessage
        {
            var context = SagaContext
                .Create()
                .WithCorrelationId(Guid.NewGuid())
                .WithOriginator("Test")
                .WithMetadata("key", "lulz")
                .Build();

            var note = JsonConvert.DeserializeObject<T>(message);

            if (!note.BelongsToSaga())
            {
                return Task.CompletedTask;
            }

            return _sagaCoordinator.ProcessAsync(note, context);
        }
    }
}
