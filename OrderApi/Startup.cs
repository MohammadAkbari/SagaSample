using Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderApi.CommandHandlers;
using RabbitMQ.Client.Core.DependencyInjection;

namespace OrderApi
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
                .AddAsyncNonCyclicMessageHandlerSingleton<RevokeOrderCommandHandler>("order.revoke")
                .AddAsyncNonCyclicMessageHandlerSingleton<ApproveOrderCommandHandler>("order.approve");

            services.AddHostedService<QueueHostedService>();
        }
         
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
