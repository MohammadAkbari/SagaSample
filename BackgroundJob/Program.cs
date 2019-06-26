using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using Serilog;
using Topshelf.Extensions.Hosting;

namespace BackgroundJob
{
    internal class Program
    {
        //dotnet publish -c Release -r win10-x64

        public static void Main(string[] args)
        {
            var hostBuilder = new HostBuilder()
                .ConfigureAppConfiguration(ConfigureAppConfiguration)
                .ConfigureServices(ConfigureServices)
                .ConfigureLogging(ConfigureLogging);

            if (Debugger.IsAttached)
            {
                hostBuilder.Build().Run();
            }
            else
            {
                hostBuilder.Build().RunAsService();

                hostBuilder.RunAsTopshelfService(hc =>
                {
                    hc.SetServiceName("GenericHostInTopshelf");
                    hc.SetDisplayName("Generic Host In Topshelf");
                    hc.SetDescription("Runs a generic host as a Topshelf service.");
                });
            }            
        }

        private static readonly Action<HostBuilderContext, IServiceCollection> ConfigureServices =
            (hostingContext, services) =>
            {
                services.AddSingleton<IJobFactory, JobFactory>();
                services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
                services.AddSingleton<QuartzJobRunner>();
                services.AddHostedService<QuartzHostedService>();

                services.AddSingleton<Service1>();
                services.AddScoped<Service2>();
                services.AddTransient<Service3>();
                services.AddTransient<Service4>();
                services.AddTransient<Service5>();

                services.AddScoped<HelloWorldJob>();

                services.AddSingleton(new JobSchedule(
                    jobType: typeof(HelloWorldJob),
                    cronExpression: "0/10 * * * * ?")); //every 10 seconds
                //cronExpression: "0 0 13 * * ?")); //every day at 13:00 
            };

        private static readonly Action<HostBuilderContext, ILoggingBuilder> ConfigureLogging =
            (hostingContext, logging) =>
            {
                var configuration = hostingContext.Configuration;

                logging.AddConfiguration(configuration.GetSection("Logging"));
                logging.AddConsole();

                logging.AddSerilog();

                Log.Logger = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .Enrich.WithProperty("Assembly", System.AppDomain.CurrentDomain.FriendlyName)
                    .WriteTo.Seq(configuration["Seq:ServerUrl"])
                    .WriteTo.ColoredConsole()
                    .CreateLogger();
            };

        private static readonly Action<HostBuilderContext, IConfigurationBuilder> ConfigureAppConfiguration =
            (hostingContext, config) => { config.AddJsonFile("appsettings.json", optional: true); };
    }


    #region Services

    internal class BaseService
    {
        private int _counter = 0;
        public int Counter => _counter++;
    }

    internal class Service4
    {
        private readonly Service1 _service1;
        private readonly Service2 _service2;
        private readonly Service3 _service3;
        private readonly ILogger<Service5> _logger;

        public Service4(Service1 service1, Service2 service2, Service3 service3, ILogger<Service5> logger)
        {
            _service1 = service1;
            _service2 = service2;
            _service3 = service3;
            _logger = logger;
        }

        public void Log()
        {
            _logger.LogInformation(
                $"_service1: {_service1.Counter}, _service2: {_service2.Counter}, _service3: {_service3.Counter}");
        }
    }

    internal class Service5
    {
        private readonly Service1 _service1;
        private readonly Service2 _service2;
        private readonly Service3 _service3;
        private readonly ILogger<Service5> _logger;

        public Service5(Service1 service1, Service2 service2, Service3 service3, ILogger<Service5> logger)
        {
            _service1 = service1;
            _service2 = service2;
            _service3 = service3;
            _logger = logger;
        }

        public void Log()
        {
            _logger.LogInformation(
                $"_service1: {_service1.Counter}, _service2: {_service2.Counter}, _service3: {_service3.Counter}");
        }
    }

    internal class Service1 : BaseService, IDisposable
    {
        private readonly ILogger<Service1> _logger;

        public Service1(ILogger<Service1> logger)
        {
            _logger = logger;
        }

        public void Dispose()
        {
            _logger.LogInformation($"_service1 Disposed");
        }
    }

    internal class Service2 : BaseService, IDisposable
    {
        private readonly ILogger<Service2> _logger;

        public Service2(ILogger<Service2> logger)
        {
            _logger = logger;
        }

        public void Dispose()
        {
            _logger.LogInformation($"_service2 Disposed");
        }
    }

    internal class Service3 : BaseService, IDisposable
    {
        private readonly ILogger<Service3> _logger;

        public Service3(ILogger<Service3> logger)
        {
            _logger = logger;
        }

        public void Dispose()
        {
            _logger.LogInformation($"_service3 Disposed");
        }
    }

    #endregion
}