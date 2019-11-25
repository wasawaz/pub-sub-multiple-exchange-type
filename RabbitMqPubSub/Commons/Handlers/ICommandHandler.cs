using System.Threading.Tasks;
using RabbitMqPubSub.Commons.Messages;
using RabbitMqPubSub.Commons.RabbitMq;

namespace RabbitMqPubSub.Commons.Handlers
{
    public interface ICommandHandler<in TCommand> where TCommand : ICommand
    {
        Task HandleAsync(TCommand command, ICorrelationContext context);
    }
}