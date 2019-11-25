using System;
using System.Threading.Tasks;
using Autofac;
using RabbitMqPubSub.Commons.Handlers;
using RabbitMqPubSub.Commons.Messages;
using RabbitMqPubSub.Commons.RabbitMq;

namespace RabbitMqPubSub.Commons.Dispatchers
{
    public class CommandDispatcher : ICommandDispatcher
    {
        private readonly IComponentContext _context;
        private readonly IServiceProvider _serviceProvider;   

        public CommandDispatcher(IComponentContext context, IServiceProvider serviceProvider)
        {
            _context = context;
            _serviceProvider = serviceProvider;
        }

        public async Task SendAsync<T>(T command) where T : ICommand
            =>await _context.Resolve<ICommandHandler<T>>().HandleAsync(command, CorrelationContext.Empty);
        
        
    }
}