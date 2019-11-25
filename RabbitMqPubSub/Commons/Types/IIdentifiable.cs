using MongoDB.Bson;

namespace RabbitMqPubSub.Commons.Types
{
    public interface IIdentifiable
    {
         ObjectId Id { get; }
    }
}