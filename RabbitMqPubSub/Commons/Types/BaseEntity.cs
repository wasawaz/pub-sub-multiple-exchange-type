using System;
using MongoDB.Bson;

namespace RabbitMqPubSub.Commons.Types
{
    public abstract class BaseEntity : IIdentifiable
    {
        public ObjectId Id { get; protected set; }
        public DateTime CreatedDate { get; protected set; } 
        public DateTime UpdatedDate { get; protected set; }

        public BaseEntity(ObjectId id)
        {
            Id = id;
            CreatedDate = DateTime.UtcNow;
            SetUpdatedDate();
        }

        protected virtual void SetUpdatedDate()
            => UpdatedDate = DateTime.UtcNow;
    }
}