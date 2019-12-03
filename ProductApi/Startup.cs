using Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProductApi.CommandHandlers;
using RabbitMQ.Client.Core.DependencyInjection;

namespace ProductApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
         
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            var rabbitMqSection = Configuration.GetSection("RabbitMq");
            var exchangeSection = Configuration.GetSection("RabbitMqExchange");

            services.AddDistributedRedisCache(option =>
            {
                option.Configuration = Configuration.GetConnectionString("Redis");
                option.InstanceName = "master";
            });

            services.AddRabbitMqClient(rabbitMqSection)
                .AddConsumptionExchange("exchange.name", exchangeSection) 
                .AddAsyncNonCyclicMessageHandlerSingleton<ReserveProductsCommandHandler>("products.reserve")
                .AddAsyncNonCyclicMessageHandlerSingleton<ReleaseProductsCommandHandler>("products.release");

            services.AddHostedService<QueueHostedService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var queueService = app.ApplicationServices.GetService<IQueueService>();
            queueService.StartConsuming();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
