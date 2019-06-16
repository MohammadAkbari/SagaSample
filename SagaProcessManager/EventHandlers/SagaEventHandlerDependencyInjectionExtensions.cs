using System;
using System.Linq;
using Common;
using Common.Events;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client.Core.DependencyInjection;

namespace SagaProcessManager.EventHandlers
{
    public static class SagaEventHandlerDependencyInjectionExtensions
    {
        public static IServiceCollection AddEventHandlers(this IServiceCollection services)
        {
            var type = typeof(IEvent);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract)
                .ToList();

            var genericAsyncMessageHandlerType = typeof(GenericAsyncEventHandler<>);

            foreach (var item in types)
            {
                var attribute = item.GetCustomAttributes(typeof(MessageNamespaceAttribute), true).FirstOrDefault() as MessageNamespaceAttribute;

                if (attribute is null)
                {
                    continue;
                }
                
                var syncMessageHandlerType = genericAsyncMessageHandlerType.MakeGenericType(item);
                var method = typeof(RabbitMqClientDependencyInjectionExtensions).GetMethod("AddAsyncMessageHandlerSingleton", new Type[] { typeof(IServiceCollection), typeof(string)});
                var generic = method.MakeGenericMethod(syncMessageHandlerType);
                generic.Invoke(services, new object[] {  services, attribute.Namespace });
            }

            return services;
        }
    }
}
