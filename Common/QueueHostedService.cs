using Microsoft.Extensions.Hosting;
using RabbitMQ.Client.Core.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;

namespace Common
{
    public class QueueHostedService : IHostedService
    {
        private readonly IQueueService _queueService;
        private readonly ILogger<QueueHostedService> _logger;

        public QueueHostedService(IQueueService queueService, ILogger<QueueHostedService> logger)
        {
            _queueService = queueService;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"start service at {DateTime.Now}");
            _queueService.StartConsuming();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
