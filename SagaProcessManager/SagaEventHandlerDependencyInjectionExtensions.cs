using Common.Events;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client.Core.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace SagaProcessManager
{
    public static class SagaEventHandlerDependencyInjectionExtensions
    {
        public static IServiceCollection AddEventHandlers(this IServiceCollection services)
        {
            var type = typeof(OrderCreated);
            //var type = typeof(IEvent);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract)
                .ToList();

            var genericAsyncMessageHandlerType = typeof(GenericAsyncMessageHandler<>);

            foreach (var item in types)
            {
                var attribute = item.GetCustomAttributes(typeof(MessageNamespaceAttribute), true).FirstOrDefault() as MessageNamespaceAttribute;

                var syncMessageHandlerType = genericAsyncMessageHandlerType.MakeGenericType(item);
                MethodInfo method = typeof(RabbitMqClientDependencyInjectionExtensions).GetMethod("AddAsyncMessageHandlerSingleton", new Type[] { typeof(IServiceCollection), typeof(string)});
                MethodInfo generic = method.MakeGenericMethod(syncMessageHandlerType);
                generic.Invoke(services, new object[] {  services, attribute.Namespace });
            }

            return services;
        }
    }
}
