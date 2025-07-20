using System.Diagnostics;
using System.Text;
using System.Text.Json;
using FastTechFoodsOrder.Api.Interfaces;
using FastTechFoodsOrder.Shared.Integration.Messages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FastTechFoodsOrder.Api.Services
{
    public class RabbitMQConsumerService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RabbitMQConsumerService> _logger;
        private readonly List<IChannel> _channels = new();
        private static readonly ActivitySource ActivitySource = new("FastTechFoodsOrder.RabbitMQConsumer");

        public RabbitMQConsumerService(
            IServiceProvider serviceProvider,
            ILogger<RabbitMQConsumerService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("🚀 RabbitMQ Consumer Service starting...");

            SetupConsumer<OrderAcceptedMessage>("order.accepted.queue");
            SetupConsumer<OrderPreparingMessage>("order.preparing.queue");
            SetupConsumer<OrderReadyMessage>("order.ready.queue");
            SetupConsumer<OrderCompletedMessage>("order.completed.queue");
            SetupConsumer<OrderCancelledMessage>("order.cancelled.queue");
            _logger.LogInformation("✅ RabbitMQ Consumer Service started successfully!");
            return Task.CompletedTask;
        }

        private async void SetupConsumer<T>(string queueName) where T : class
        {
            using var scope = _serviceProvider.CreateScope();
            var connection = scope.ServiceProvider.GetRequiredService<IConnection>();
            var channel = await connection.CreateChannelAsync();
            
            await channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                using var activity = ActivitySource.StartActivity("rabbitmq.message_received");
                activity?.SetTag("rabbitmq.queue", queueName);
                activity?.SetTag("rabbitmq.message_type", typeof(T).Name);

                var body = ea.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);

                _logger.LogInformation("📨 Message received from queue: {QueueName}, Type: {MessageType}, Size: {MessageSize} bytes", 
                    queueName, typeof(T).Name, body.Length);

                try
                {
                    // Extrair trace context se disponível
                    if (ea.BasicProperties?.Headers != null)
                    {
                        if (ea.BasicProperties.Headers.TryGetValue("trace-id", out var traceIdBytes) && traceIdBytes is byte[] bytes)
                        {
                            var traceId = Encoding.UTF8.GetString(bytes);
                            activity?.SetTag("trace.parent_id", traceId);
                        }
                    }

                    activity?.SetTag("message.content", messageJson);
                    
                    var message = JsonSerializer.Deserialize<T>(messageJson);
                    if (message == null)
                    {
                        throw new InvalidOperationException($"Failed to deserialize message to type {typeof(T).Name}");
                    }
                    
                    using var processingScope = _serviceProvider.CreateScope();
                    var handler = processingScope.ServiceProvider.GetRequiredService<IMessageHandler<T>>();
                    
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                    
                    _logger.LogInformation("🔄 Starting to process message from queue {QueueName} with type {MessageType}", queueName, typeof(T).Name);
                    
                    await handler.HandleAsync(message, activity).WaitAsync(cts.Token);
                    
                    _logger.LogInformation("✅ Message processing completed for queue {QueueName}", queueName);

                    await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                    activity?.SetTag("rabbitmq.status", "acknowledged");
                    
                    _logger.LogInformation("✅ Successfully processed and acknowledged message from queue {QueueName}", queueName);
                }
                catch (OperationCanceledException ex) when (ex.CancellationToken.IsCancellationRequested)
                {
                    activity?.SetTag("rabbitmq.status", "timeout");
                    activity?.SetTag("rabbitmq.error", "Processing timeout after 30 seconds");
                    
                    _logger.LogError("⏰ Message processing timeout after 30 seconds for queue {QueueName}: {Message}", queueName, messageJson);
                    
                    await channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                }
                catch (Exception ex)
                {
                    activity?.SetTag("rabbitmq.status", "error");
                    activity?.SetTag("rabbitmq.error", ex.Message);
                    
                    _logger.LogError(ex, "❌ Error processing message from queue {QueueName}: {Message}", queueName, messageJson);
                    
                    var shouldRequeue = !ex.Message.Contains("Failed to deserialize"); // Não requeue erros de deserialização
                    await channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: shouldRequeue);
                    
                    _logger.LogWarning("📝 Message {Requeued} for queue {QueueName}", 
                        shouldRequeue ? "requeued" : "rejected (not requeued)", queueName);
                }
            };

            await channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);
            _channels.Add(channel);
            
            _logger.LogInformation("🔗 Consumer configured for queue {QueueName} with message type {MessageType}", queueName, typeof(T).Name);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("🛑 RabbitMQ Consumer Service stopping...");

            foreach (var channel in _channels)
            {
                try
                {
                    await channel.CloseAsync();
                    channel?.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error closing RabbitMQ channel");
                }
            }

            _channels.Clear();
            ActivitySource?.Dispose();
            
            _logger.LogInformation("🛑 RabbitMQ Consumer Service stopped");
        }
    }
}
