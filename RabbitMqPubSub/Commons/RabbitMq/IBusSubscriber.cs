using System;
using RabbitMqPubSub.Commons.Messages;
using RabbitMqPubSub.Commons.Types;
using RawRabbit.Configuration.Exchange;

namespace RabbitMqPubSub.Commons.RabbitMq
{
    public interface IBusSubscriber
    {
        IBusSubscriber SubscribeCommand<TCommand>(string @namespace = null, string queueName = null,
            Func<TCommand, RabbitMqPubSubException, IRejectedEvent> onError = null)
            where TCommand : ICommand;

        IBusSubscriber SubscribeEvent<TEvent>(string @namespace = null, string queueName = null,string prefixQueueName = null,
            ExchangeType exchangeType = ExchangeType.Unknown,
            Func<TEvent, RabbitMqPubSubException, IRejectedEvent> onError = null)
            where TEvent : IEvent;
    }
}