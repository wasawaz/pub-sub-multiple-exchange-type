using System.Threading.Tasks;
using RabbitMqPubSub.Commons.Messages;
using RabbitMqPubSub.Commons.Types;

namespace RabbitMqPubSub.Commons.Dispatchers
{
    public interface IDispatcher
    {
        Task SendAsync<TCommand>(TCommand command) where TCommand : ICommand;
        Task<TResult> QueryAsync<TResult>(IQuery<TResult> query);
    }
}