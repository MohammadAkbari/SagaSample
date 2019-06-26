using System.ServiceProcess;
using Microsoft.Extensions.Hosting;

namespace BackgroundJob
{
    public static class GenericHostWindowsServiceExtensions
    {
        public static void RunAsService(this IHost host)
        {
            var hostService = new GenericServiceHost(host);
            ServiceBase.Run(hostService);
        }
    }
}