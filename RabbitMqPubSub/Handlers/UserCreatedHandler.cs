using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMqPubSub.Commons.Handlers;
using RabbitMqPubSub.Commons.RabbitMq;
using RabbitMqPubSub.Messages.Events.Subscribe;

namespace RabbitMqPubSub.Handlers
{
    public class UserCreatedHandler : IEventHandler<UserCreated>
    {
        private ILogger<UserCreatedHandler> _logger;

        public UserCreatedHandler(ILogger<UserCreatedHandler> logger)
        {
            _logger = logger;
        }

        public Task HandleAsync(UserCreated @event, ICorrelationContext context)
        {
            _logger.LogInformation("User Received Message !!!");
            return Task.CompletedTask;
        }
    }
}