﻿using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using RabbitMqPubSub.Commons.Handlers;
using RabbitMqPubSub.Commons.Messages;
using RabbitMqPubSub.Commons.Types;
using RawRabbit;
using RawRabbit.Common;
using RawRabbit.Configuration.Exchange;
using RawRabbit.Enrichers.MessageContext;

namespace RabbitMqPubSub.Commons.RabbitMq
{
    public class BusSubscriber : IBusSubscriber
    {
        private readonly ILogger _logger;
        private readonly IBusClient _busClient;
        private readonly IServiceProvider _serviceProvider;
        private readonly string _defaultNamespace;
        private readonly int _retries;
        private readonly int _retryInterval;

        public BusSubscriber(IApplicationBuilder app)
        {
            _logger = app.ApplicationServices.GetService<ILogger<BusSubscriber>>();
            _serviceProvider = app.ApplicationServices.GetService<IServiceProvider>();
            _busClient = _serviceProvider.GetService<IBusClient>();
            var options = _serviceProvider.GetService<RabbitMqOptions>();
            _defaultNamespace = options.Namespace;
            _retries = options.Retries >= 0 ? options.Retries : 3;
            _retryInterval = options.RetryInterval > 0 ? options.RetryInterval : 2;
        }

        public IBusSubscriber SubscribeCommand<TCommand>(string @namespace = null, string queueName = null,
            Func<TCommand, RabbitMqPubSubException, IRejectedEvent> onError = null)
            where TCommand : ICommand
        {
            _busClient.SubscribeAsync<TCommand, CorrelationContext>(async (command, correlationContext) =>
                {
                    var commandHandler = _serviceProvider.GetService<ICommandHandler<TCommand>>();

                    return await TryHandleAsync(command, correlationContext,
                        () => commandHandler.HandleAsync(command, correlationContext), onError);
                },
                ctx => ctx.UseSubscribeConfiguration(cfg =>
                    cfg.FromDeclaredQueue(q => q.WithName(GetQueueName<TCommand>(@namespace, queueName)))));

            return this;
        }

        public IBusSubscriber SubscribeEvent<TEvent>(string @namespace = null, string queueName = null,
            string prefixQueueName = null,
            ExchangeType exchangeType = ExchangeType.Unknown,
            Func<TEvent, RabbitMqPubSubException, IRejectedEvent> onError = null)
            where TEvent : IEvent
        {
            _busClient.SubscribeAsync<TEvent, CorrelationContext>(async (@event, correlationContext) =>
                {
                    var eventHandler = _serviceProvider.GetService<IEventHandler<TEvent>>();

                    return await TryHandleAsync(@event, correlationContext,
                        () => eventHandler.HandleAsync(@event, correlationContext), onError);
                },
                ctx => ctx.UseSubscribeConfiguration((cfg) =>
                    {
                        cfg.FromDeclaredQueue(q => q.WithName(GetQueueName<TEvent>(@namespace, queueName,prefixQueueName)));
                        if (exchangeType != ExchangeType.Unknown)
                            cfg.OnDeclaredExchange(e => e.WithType(exchangeType).WithName(@namespace));
                    }
                ));

            return this;
        }

        // Internal retry for services that subscribe to the multiple events of the same types.
        // It does not interfere with the routing keys and wildcards (see TryHandleWithRequeuingAsync() below).
        private async Task<Acknowledgement> TryHandleAsync<TMessage>(TMessage message,
            CorrelationContext correlationContext,
            Func<Task> handle, Func<TMessage, RabbitMqPubSubException, IRejectedEvent> onError = null)
        {
            var currentRetry = 0;
            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(_retries, i => TimeSpan.FromSeconds(_retryInterval));

            var messageName = message.GetType().Name;

            return await retryPolicy.ExecuteAsync<Acknowledgement>(async () =>
            {
                try
                {
                    var retryMessage = currentRetry == 0
                        ? string.Empty
                        : $"Retry: {currentRetry}'.";

                    var preLogMessage = $"Handling a message: '{messageName}' " +
                                        $"with correlation id: '{correlationContext.Id}'. {retryMessage}";

                    _logger.LogInformation(preLogMessage);

                    await handle();

                    var postLogMessage = $"Handled a message: '{messageName}' " +
                                         $"with correlation id: '{correlationContext.Id}'. {retryMessage}";
                    _logger.LogInformation(postLogMessage);

                    return new Ack();
                }
                catch (Exception exception)
                {
                    currentRetry++;
                    _logger.LogError(exception, exception.Message);

                    if (exception is RabbitMqPubSubException notificationException && onError != null)
                    {
                        var rejectedEvent = onError(message, notificationException);
                        await _busClient.PublishAsync(rejectedEvent, ctx => ctx.UseMessageContext(correlationContext));
                        _logger.LogInformation($"Published a rejected event: '{rejectedEvent.GetType().Name}' " +
                                               $"for the message: '{messageName}' with correlation id: '{correlationContext.Id}'.");
                    }

                    throw new Exception($"Unable to handle a message: '{messageName}' " +
                                        $"with correlation id: '{correlationContext.Id}', " +
                                        $"retry {currentRetry - 1}/{_retries}...");
                }
            });
        }

        // RabbitMQ retry that will publish a message to the retry queue.
        // Keep in mind that it might get processed by the other services using the same routing key and wildcards.
        private async Task<Acknowledgement> TryHandleWithRequeuingAsync<TMessage>(TMessage message,
            CorrelationContext correlationContext,
            Func<Task> handle, Func<TMessage, RabbitMqPubSubException, IRejectedEvent> onError = null)
        {
            var messageName = message.GetType().Name;
            var retryMessage = correlationContext.Retries == 0
                ? string.Empty
                : $"Retry: {correlationContext.Retries}'.";
            _logger.LogInformation($"Handling a message: '{messageName}' " +
                                   $"with correlation id: '{correlationContext.Id}'. {retryMessage}");

            try
            {
                await handle();
                _logger.LogInformation($"Handled a message: '{messageName}' " +
                                       $"with correlation id: '{correlationContext.Id}'. {retryMessage}");

                return new Ack();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, exception.Message);
                if (exception is RabbitMqPubSubException notificationException && onError != null)
                {
                    var rejectedEvent = onError(message, notificationException);
                    await _busClient.PublishAsync(rejectedEvent, ctx => ctx.UseMessageContext(correlationContext));
                    _logger.LogInformation($"Published a rejected event: '{rejectedEvent.GetType().Name}' " +
                                           $"for the message: '{messageName}' with correlation id: '{correlationContext.Id}'.");

                    return new Ack();
                }

                if (correlationContext.Retries >= _retries)
                {
                    await _busClient.PublishAsync(RejectedEvent.For(messageName),
                        ctx => ctx.UseMessageContext(correlationContext));

                    throw new Exception($"Unable to handle a message: '{messageName}' " +
                                        $"with correlation id: '{correlationContext.Id}' " +
                                        $"after {correlationContext.Retries} retries.", exception);
                }

                _logger.LogInformation($"Unable to handle a message: '{messageName}' " +
                                       $"with correlation id: '{correlationContext.Id}', " +
                                       $"retry {correlationContext.Retries}/{_retries}...");

                return Retry.In(TimeSpan.FromSeconds(_retryInterval));
            }
        }

        private string GetQueueName<T>(string @namespace = null, string name = null, string prefix = null)
        {
            @namespace = string.IsNullOrWhiteSpace(@namespace)
                ? (string.IsNullOrWhiteSpace(_defaultNamespace) ? string.Empty : _defaultNamespace)
                : @namespace;

            var separatedNamespace = string.IsNullOrWhiteSpace(@namespace) ? string.Empty : $"{@namespace}.";
            var prefixQueue = string.IsNullOrWhiteSpace(prefix) ? string.Empty : $"{prefix}.";
            return (string.IsNullOrWhiteSpace(name)
                ? $"{Assembly.GetEntryAssembly().GetName().Name}/{prefixQueue}{separatedNamespace}{typeof(T).Name.Underscore()}"
                : $"{name}/{prefixQueue}{separatedNamespace}{typeof(T).Name.Underscore()}").ToLowerInvariant();
        }
    }
}