using System;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nest;

namespace ElasticsearchApp
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            
            var builder = new HostBuilder()
                .ConfigureAppConfiguration(ConfigureAppConfiguration)
                .ConfigureServices(ConfigureServices)
                .ConfigureLogging(ConfigureLogging);
            
            builder.RunConsoleAsync().GetAwaiter().GetResult();
        }
        
        private static readonly Action<HostBuilderContext, IServiceCollection> ConfigureServices = (hostContext, services) =>
        {
            var configuration = hostContext.Configuration;

            var url = configuration.GetConnectionString("Elastic");

            services.AddHostedService<ApplicationHostedService>();
            services.AddSingleton<IElasticClient>(GetElasticClient(url));
            services.AddSingleton<PostSearchService>();
        };
        
        private static readonly Action<HostBuilderContext, ILoggingBuilder> ConfigureLogging = (hostingContext, logging) =>
        {
            var configuration = hostingContext.Configuration;

            logging.AddConfiguration(configuration.GetSection("Logging"));
                      
            logging.AddConsole();
        };
        
        private static readonly Action<HostBuilderContext, IConfigurationBuilder> ConfigureAppConfiguration = (hostingContext, config) =>
        {
            config.AddJsonFile("appsettings.json", optional: true);
        };
        
        private static ElasticClient GetElasticClient(string elasticUriString)
        {
            var connectionSettings = new ConnectionSettings(new Uri(elasticUriString));
            connectionSettings.DefaultIndex("stackoverflow");
            connectionSettings.DisableDirectStreaming(false);

            return new ElasticClient(connectionSettings);
        }
    }
}
