﻿using Chronicle;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client.Core.DependencyInjection;
using SagaProcessManager.EventHandlers;
using System;
using System.IO;

namespace SagaProcessManager
{
    class Program
    {
        public static IConfiguration Configuration { get; set; }

        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            Configuration = builder.Build();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var queueService = serviceProvider.GetRequiredService<IQueueService>();
            queueService.StartConsuming();

            Console.WriteLine("Start Consuming");
        }

        static void ConfigureServices(IServiceCollection services)
        {
            services.AddChronicle();

            var rabbitMqSection = Configuration.GetSection("RabbitMq");
            var exchangeSection = Configuration.GetSection("RabbitMqExchange");

            services.AddRabbitMqClient(rabbitMqSection)
                .AddExchange("exchange.name", exchangeSection)
                .AddAsyncMessageHandlerSingleton<OrderCreatedMessageHandler>("event.order-created")
                .AddAsyncMessageHandlerSingleton<OrderRevokedMessageHandler>("event.order-revoked")
                .AddAsyncMessageHandlerSingleton<RevokeOrderRejectedMessageHandler>("event.revoke-order-rejected")
                .AddAsyncMessageHandlerSingleton<ProductsReservedMessageHandler>("event.products-reserved")
                .AddAsyncMessageHandlerSingleton<ReserveProductsRejectedMessageHandler>("event.reserve-products-rejected")
                .AddAsyncMessageHandlerSingleton<OrderApprovedMessageHandler>("event.order-approved")
                .AddAsyncMessageHandlerSingleton<ApproveOrderRejectedMessageHandler>("event.approve-order-rejected");
        }
    }
}