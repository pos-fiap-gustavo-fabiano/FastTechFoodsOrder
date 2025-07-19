using FastTechFoodsOrder.Application.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace FastTechFoodsOrder.Application.Services
{
    public class RabbitMQPublisher : IRabbitMQPublisher
    {
        private readonly IChannel _channel;
        private readonly ILogger<RabbitMQPublisher> _logger;
        private static readonly ActivitySource ActivitySource = new("FastTechFoodsOrder.RabbitMQPublisher");

        public RabbitMQPublisher(IChannel channel, ILogger<RabbitMQPublisher> logger)
        {
            _channel = channel;
            _logger = logger;
        }

        public async Task PublishAsync<T>(T message, string exchange, string routingKey) where T : class
        {
            using var activity = ActivitySource.StartActivity("rabbitmq.publish");
            activity?.SetTag("rabbitmq.exchange", exchange);
            activity?.SetTag("rabbitmq.routing_key", routingKey);
            activity?.SetTag("message.type", typeof(T).Name);

            try
            {
                // Declara exchange se não existir
                await _channel.ExchangeDeclareAsync(exchange, ExchangeType.Fanout, durable: true, autoDelete: false);

                var messageBody = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(messageBody);

                var properties = new BasicProperties
                {
                    Persistent = true, // Mensagens persistentes
                    ContentType = "application/json",
                    MessageId = Guid.NewGuid().ToString(),
                    Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                };
                
                // Adiciona trace context para correlação
                if (Activity.Current != null)
                {
                    properties.Headers = new Dictionary<string, object?>
                    {
                        ["trace-id"] = Activity.Current.TraceId.ToString(),
                        ["span-id"] = Activity.Current.SpanId.ToString()
                    };
                }

                await _channel.BasicPublishAsync(
                    exchange: exchange,
                    routingKey: routingKey,
                    body: body,
                    mandatory: false,
                    basicProperties: properties);

                activity?.SetTag("rabbitmq.status", "published");
                _logger.LogInformation("Message {MessageType} published to exchange {Exchange} with routing key {RoutingKey}",
                    typeof(T).Name, exchange, routingKey);
            }
            catch (Exception ex)
            {
                activity?.SetTag("rabbitmq.status", "error");
                activity?.SetTag("rabbitmq.error", ex.Message);
                _logger.LogError(ex, "Failed to publish message {MessageType} to exchange {Exchange}",
                    typeof(T).Name, exchange);
                throw;
            }
        }

        public async Task PublishAsync<T>(T message, string queueName) where T : class
        {
            using var activity = ActivitySource.StartActivity("rabbitmq.publish_to_queue");
            activity?.SetTag("rabbitmq.queue", queueName);
            activity?.SetTag("message.type", typeof(T).Name);

            try
            {
                // Declara fila se não existir
                await _channel.QueueDeclareAsync(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var messageBody = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(messageBody);

                var properties = new BasicProperties
                {
                    Persistent = true,
                    ContentType = "application/json",
                    MessageId = Guid.NewGuid().ToString(),
                    Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                };

                // Adiciona trace context
                if (Activity.Current != null)
                {
                    properties.Headers = new Dictionary<string, object?>
                    {
                        ["trace-id"] = Activity.Current.TraceId.ToString(),
                        ["span-id"] = Activity.Current.SpanId.ToString()
                    };
                }

                await _channel.BasicPublishAsync(
                    exchange: string.Empty, // Default exchange
                    routingKey: queueName,
                    body: body,
                    mandatory: false,
                    basicProperties: properties);

                activity?.SetTag("rabbitmq.status", "published");
                _logger.LogInformation("Message {MessageType} published to queue {QueueName}",
                    typeof(T).Name, queueName);
            }
            catch (Exception ex)
            {
                activity?.SetTag("rabbitmq.status", "error");
                activity?.SetTag("rabbitmq.error", ex.Message);
                _logger.LogError(ex, "Failed to publish message {MessageType} to queue {QueueName}",
                    typeof(T).Name, queueName);
                throw;
            }
        }
    }
}
