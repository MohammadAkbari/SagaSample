using System.Threading.Tasks;

namespace Common
{
    public interface IBusPublisher
    {
        Task SendAsync<TCommand>(TCommand command) where TCommand : class, ICommand;

        Task PublishAsync<TEvent>(TEvent @event) where TEvent : class, IEvent;
    }
}
