using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;

namespace BackgroundJob
{
    class Program
    {
        public static void Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureAppConfiguration(ConfigureAppConfiguration)
                .ConfigureServices(ConfigureServices)
                .ConfigureLogging(ConfigureLogging)
                .Build();

            if (Debugger.IsAttached)
            {
                host.Run();
            }
            else
            {
                host.RunAsService();
            }
        }

        private static readonly Action<HostBuilderContext, IServiceCollection> ConfigureServices = (hostingContext, services) =>
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
    }
    
    public class JobFactory : IJobFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public JobFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            return _serviceProvider.GetRequiredService<QuartzJobRunner>();
        }

        public void ReturnJob(IJob job)
        {
            // we let the DI container handler this
        }
    }
    
    [DisallowConcurrentExecution]
    public class QuartzJobRunner : IJob
    {
        private readonly IServiceProvider _serviceProvider;

        public QuartzJobRunner(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var job = scope.ServiceProvider.GetRequiredService(context.JobDetail.JobType) as IJob;

                if (job != null) await job.Execute(context);
            }
        }
    }
    
    public class HelloWorldJob : IJob
    {
        private readonly Service4 _service4;
        private readonly Service5 _service5;

        public HelloWorldJob(ILogger<HelloWorldJob> logger, Service4 service4, Service5 service5)
        {
            _service4 = service4;
            _service5 = service5;
        }

        public Task Execute(IJobExecutionContext context)
        {
            _service4.Log();
            _service5.Log();
            return Task.CompletedTask;
        }
    }
    
    public class QuartzHostedService : IHostedService
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly IJobFactory _jobFactory;
        private readonly IEnumerable<JobSchedule> _jobSchedules;
        private IScheduler _scheduler;

        public QuartzHostedService(
            ISchedulerFactory schedulerFactory,
            IEnumerable<JobSchedule> jobSchedules,
            IJobFactory jobFactory)
        {
            _schedulerFactory = schedulerFactory;
            _jobSchedules = jobSchedules;
            _jobFactory = jobFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
            _scheduler.JobFactory = _jobFactory;

            foreach (var jobSchedule in _jobSchedules)
            {
                var job = CreateJob(jobSchedule);
                var trigger = CreateTrigger(jobSchedule);

                await _scheduler.ScheduleJob(job, trigger, cancellationToken);
            }

            await _scheduler.Start(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _scheduler.Shutdown(cancellationToken);
        }

        private static ITrigger CreateTrigger(JobSchedule schedule)
        {
            return TriggerBuilder
                .Create()
                .WithIdentity($"{schedule.JobType.FullName}.trigger")
                .WithCronSchedule(schedule.CronExpression)
                .WithDescription(schedule.CronExpression)
                .Build();
        }

        private static IJobDetail CreateJob(JobSchedule schedule)
        {
            var jobType = schedule.JobType;
            return JobBuilder
                .Create(jobType)
                .WithIdentity(jobType.FullName)
                .WithDescription(jobType.Name)
                .Build();
        }
    }
    
    public class JobSchedule
    {
        public JobSchedule(Type jobType, string cronExpression)
        {
            JobType = jobType;
            CronExpression = cronExpression;
        }

        public Type JobType { get; }
        public string CronExpression { get; }
    }

    #region Services

    public class BaseService
    {
        private int _counter = 0;
        public int Counter => _counter++;
    }

    public class Service4
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
            _logger.LogInformation($"_service1: {_service1.Counter}, _service2: {_service2.Counter}, _service3: {_service3.Counter}");
        }
    }

    public class Service5
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
            _logger.LogInformation($"_service1: {_service1.Counter}, _service2: {_service2.Counter}, _service3: {_service3.Counter}");
        }
    }

    public class Service1 : BaseService, IDisposable
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

    public class Service2 : BaseService, IDisposable
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

    public class Service3 : BaseService, IDisposable
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


    public class GenericServiceHost : ServiceBase
    {
        private IHost _host;
        private bool _stopRequestedByWindows;

        public GenericServiceHost(IHost host)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
        }

        protected sealed override void OnStart(string[] args)
        {
            OnStarting(args);

            _host
                .Services
                .GetRequiredService<Microsoft.Extensions.Hosting.IApplicationLifetime>()
                .ApplicationStopped
                .Register(() =>
                {
                    if (!_stopRequestedByWindows)
                    {
                        Stop();
                    }
                });

            _host.Start();

            OnStarted();
        }

        protected sealed override void OnStop()
        {
            _stopRequestedByWindows = true;
            OnStopping();
            try
            {
                _host.StopAsync().GetAwaiter().GetResult();
            }
            finally
            {
                _host.Dispose();
                OnStopped();
            }
        }

        protected virtual void OnStarting(string[] args) { }

        protected virtual void OnStarted() { }

        protected virtual void OnStopping() { }

        protected virtual void OnStopped() { }
    }

    public static class GenericHostWindowsServiceExtensions
    {
        public static void RunAsService(this IHost host)
        {
            var hostService = new GenericServiceHost(host);
            ServiceBase.Run(hostService);
        }
    }
}