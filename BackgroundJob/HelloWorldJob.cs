using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;

namespace BackgroundJob
{
    internal class HelloWorldJob : IJob
    {
        private readonly Service4 _service4;
        private readonly Service5 _service5;

        public HelloWorldJob(Service4 service4, Service5 service5)
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
}