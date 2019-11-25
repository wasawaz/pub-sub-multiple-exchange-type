using System.Threading.Tasks;
using RabbitMqPubSub.Commons.Handlers;
using RabbitMqPubSub.Commons.RabbitMq;
using RabbitMqPubSub.Messages.Commands;
using RabbitMqPubSub.Messages.Events.Publish;

namespace RabbitMqPubSub.Handlers
{
    public class UserCreateHandler : ICommandHandler<UserCreate>
    {
        private IBusPublisher _busPublisher;

        public UserCreateHandler(IBusPublisher busPublisher)
        {
            _busPublisher = busPublisher;
        }

        public Task HandleAsync(UserCreate command, ICorrelationContext context)
        {
            _busPublisher.PublishAsync(new UserCreated(), CorrelationContext.Empty);
            return Task.CompletedTask;
        }
    }
}