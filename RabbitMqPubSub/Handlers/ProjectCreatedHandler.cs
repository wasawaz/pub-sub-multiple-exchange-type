using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMqPubSub.Commons.Handlers;
using RabbitMqPubSub.Commons.RabbitMq;
using RabbitMqPubSub.Messages;
using RabbitMqPubSub.Messages.Events;
using RabbitMqPubSub.Messages.Events.Subscribe;

namespace RabbitMqPubSub.Handlers
{
    public class ProjectCreatedHandler : IEventHandler<ProjectCreated>
    {
        private ILogger<ProjectCreatedHandler> _logger;

        public ProjectCreatedHandler(ILogger<ProjectCreatedHandler> logger)
        {
            _logger = logger;
        }

        public Task HandleAsync(ProjectCreated @event, ICorrelationContext context)
        {
            _logger.LogInformation("Project Received Message !!!");
            return Task.CompletedTask;
        }
    }
}