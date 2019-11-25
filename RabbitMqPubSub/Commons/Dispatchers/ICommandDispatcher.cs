using System.Threading.Tasks;
using RabbitMqPubSub.Commons.Messages;

namespace RabbitMqPubSub.Commons.Dispatchers
{
    public interface ICommandDispatcher
    {
         Task SendAsync<T>(T command) where T : ICommand;
    }
}