using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Redis.ResponseCache;
using StackExchange.Redis;

namespace WebApplication1
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
            var redisConnection = Configuration.GetConnectionString("Redis");

            services.Configure<DataRedisCacheOptions>(config =>
            {
                config.Configuration = redisConnection;
                config.InstanceName = "instance1";
            });

            services.AddSingleton<IDataDistributedCache, DataDistributedCache>();

            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnection));

            services.AddOptions();
            services.Configure<RedisResponseCachingOptions>(options => options.RedisConnectionMultiplexerConfiguration = redisConnection); 

            //services.AddMemoryCache();
            services.AddResponseCaching();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.Use(async (ctx, next) =>
                {
                    ctx.Request.GetTypedHeaders().CacheControl = new CacheControlHeaderValue()
                    {
                        Public = true,
                        MaxAge = TimeSpan.FromSeconds(60)
                    };
                    await next();
                }
            );

            //app.UseResponseCaching();
            app.UseMiddleware<RedisResponseCachingMiddleware>();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }

    public interface IDataDistributedCache : IDistributedCache
    {
    }

    public class DataRedisCacheOptions : RedisCacheOptions
    {
    }

    public class DataDistributedCache : RedisCache, IDataDistributedCache
    {
        public DataDistributedCache(IOptions<DataRedisCacheOptions> optionsAccessor) : base(optionsAccessor)
        {
        }
    }
}
