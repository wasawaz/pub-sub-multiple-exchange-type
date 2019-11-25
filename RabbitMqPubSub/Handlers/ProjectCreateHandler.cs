using System.Threading.Tasks;
using RabbitMqPubSub.Commons.Handlers;
using RabbitMqPubSub.Commons.RabbitMq;
using RabbitMqPubSub.Messages.Commands;
using RabbitMqPubSub.Messages.Events.Publish;

namespace RabbitMqPubSub.Handlers
{
    public class ProjectCreateHandler : ICommandHandler<ProjectCreate>
    {
        private IBusPublisher _busPublisher;

        public ProjectCreateHandler(IBusPublisher busPublisher)
        {
            _busPublisher = busPublisher;
        }

        public async Task HandleAsync(ProjectCreate command, ICorrelationContext context)
        {
            await _busPublisher.PublishAsync(new ProjectCreated(), CorrelationContext.Empty);
        }
    }
}