using System;
using Chronicle;
using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Core.DependencyInjection;
using SagaProcessManager.EventHandlers;
using Serilog;

namespace SagaProcessManager
{
    class Program
    {

        
        public static void Main(string[] args)
        {

            var builder = new HostBuilder()
                  .ConfigureAppConfiguration(ConfigureAppConfiguration)
                  .ConfigureServices(ConfigureServices)
                  .ConfigureLogging(ConfigureLogging);
            
            builder.RunConsoleAsync().GetAwaiter().GetResult();
        }
        
        private static readonly Action<HostBuilderContext, IServiceCollection> ConfigureServices = (hostContext, services) =>
        {
            var configuration = hostContext.Configuration;

            services.AddChronicle();

            var rabbitMqSection = configuration.GetSection("RabbitMq");
            var exchangeSection = configuration.GetSection("RabbitMqExchange");

            services.AddRabbitMqClient(rabbitMqSection)
                .AddExchange("exchange.name", exchangeSection)
                .AddEventHandlers();

            services.AddHostedService<QueueHostedService>();
        };
        
        private static readonly Action<HostBuilderContext, ILoggingBuilder> ConfigureLogging = (hostingContext, logging) =>
        {
            var configuration = hostingContext.Configuration;

            logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                      
            logging.AddSerilog();
                      
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithProperty("Assembly", System.AppDomain.CurrentDomain.FriendlyName)
                .WriteTo.Seq(configuration["Seq:ServerUrl"])
                .CreateLogger();
        };
        
        private static readonly Action<HostBuilderContext, IConfigurationBuilder> ConfigureAppConfiguration = (hostingContext, config) =>
        {
            config.AddJsonFile("appsettings.json", optional: true);
        };
    }
}
