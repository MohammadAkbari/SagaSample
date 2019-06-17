using Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductApi.CommandHandlers;
using RabbitMQ.Client.Core.DependencyInjection;
using IHostedService = Microsoft.Extensions.Hosting.IHostedService;

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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            var rabbitMqSection = Configuration.GetSection("RabbitMq");
            var exchangeSection = Configuration.GetSection("RabbitMqExchange");

            services.AddDistributedRedisCache(option =>
            {
                option.Configuration = Configuration.GetConnectionString("Redis");
                option.InstanceName = "master";
            });

            services.AddRabbitMqClient(rabbitMqSection)
                .AddExchange("exchange.name", exchangeSection) 
                .AddAsyncNonCyclicMessageHandlerSingleton<ReserveProductsCommandHandler>("products.reserve")
                .AddAsyncNonCyclicMessageHandlerSingleton<ReleaseProductsCommandHandler>("products.release");

            services.AddHostedService<QueueHostedService>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            var queueService = app.ApplicationServices.GetService<IQueueService>();
            queueService.StartConsuming();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();

        }
    }
}
