using System.Threading.Tasks;
using RabbitMqPubSub.Commons.Types;

namespace RabbitMqPubSub.Commons.Handlers
{
    public interface IQueryHandler<TQuery,TResult> where TQuery : IQuery<TResult>
    {
        Task<TResult> HandleAsync(TQuery query);
    }
}