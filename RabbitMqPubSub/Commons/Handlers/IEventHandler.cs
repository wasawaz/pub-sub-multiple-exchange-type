using System.Threading.Tasks;
using RabbitMqPubSub.Commons.Messages;
using RabbitMqPubSub.Commons.RabbitMq;

namespace RabbitMqPubSub.Commons.Handlers
{
    public interface IEventHandler<in TEvent> where TEvent : IEvent
    {
        Task HandleAsync(TEvent @event, ICorrelationContext context);
    }
}