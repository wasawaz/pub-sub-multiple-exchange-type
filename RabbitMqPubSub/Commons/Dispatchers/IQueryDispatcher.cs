using System.Threading.Tasks;
using RabbitMqPubSub.Commons.Types;

namespace RabbitMqPubSub.Commons.Dispatchers
{
    public interface IQueryDispatcher
    {
        Task<TResult> QueryAsync<TResult>(IQuery<TResult> query);
    }
}