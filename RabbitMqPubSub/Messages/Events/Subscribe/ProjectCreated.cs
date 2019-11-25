using RabbitMqPubSub.Commons.Messages;

namespace RabbitMqPubSub.Messages.Events.Subscribe
{
    [MessageNamespace("projects")]
    public class ProjectCreated : IEvent
    {
        
    }
}