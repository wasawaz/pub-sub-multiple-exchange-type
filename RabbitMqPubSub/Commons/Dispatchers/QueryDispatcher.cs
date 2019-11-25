using System.Threading.Tasks;
using Autofac;
using RabbitMqPubSub.Commons.Handlers;
using RabbitMqPubSub.Commons.Types;

namespace RabbitMqPubSub.Commons.Dispatchers
{
    public class QueryDispatcher : IQueryDispatcher
    {
        private readonly IComponentContext _context;

        public QueryDispatcher(IComponentContext context)
        {
            _context = context;
        }

        public async Task<TResult> QueryAsync<TResult>(IQuery<TResult> query)
        {
            var handlerType = typeof(IQueryHandler<,>)
                .MakeGenericType(query.GetType(), typeof(TResult));

            dynamic handler = _context.Resolve(handlerType);

            return await handler.HandleAsync((dynamic)query);
        }
    }
}