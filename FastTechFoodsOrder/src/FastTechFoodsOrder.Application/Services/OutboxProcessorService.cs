using FastTechFoodsOrder.Application.Interfaces;
using FastTechFoodsOrder.Shared.Messages;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace FastTechFoodsOrder.Application.Services
{
    public class OutboxProcessorService : IHostedService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OutboxProcessorService> _logger;
        private Timer? _timer;
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(30); // Processar a cada 30 segundos
        
        // OpenTelemetry ActivitySource para tracing
        private static readonly ActivitySource ActivitySource = new("FastTechFoodsOrder.OutboxProcessor");

        public OutboxProcessorService(
            IServiceProvider serviceProvider,
            ILogger<OutboxProcessorService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Outbox Processor Service started");
            _timer = new Timer(ProcessOutboxEvents, null, TimeSpan.Zero, _interval);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Outbox Processor Service stopped");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        private async void ProcessOutboxEvents(object? state)
        {
            using var activity = ActivitySource.StartActivity("outbox.process_batch");
            using var scope = _serviceProvider.CreateScope();
            var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

            try
            {
                var events = await outboxRepository.GetUnprocessedEventsAsync();
                activity?.SetTag("outbox.events_count", events.Count());
                
                foreach (var outboxEvent in events)
                {
                    using var eventActivity = ActivitySource.StartActivity("outbox.process_event");
                    eventActivity?.SetTag("outbox.event_id", outboxEvent.Id);
                    eventActivity?.SetTag("outbox.event_type", outboxEvent.EventType);
                    eventActivity?.SetTag("outbox.retry_count", outboxEvent.RetryCount);
                    
                    try
                    {
                        // 1. Publica o evento
                        await PublishEvent(outboxEvent, publishEndpoint);
                        
                        // 2. Aguarda confirmação de entrega do RabbitMQ
                        // Se chegou aqui, o RabbitMQ confirmou que recebeu a mensagem
                        await outboxRepository.MarkAsProcessedAsync(outboxEvent.Id);
                        
                        eventActivity?.SetTag("outbox.status", "success");
                        _logger.LogInformation("Successfully processed outbox event {EventId} of type {EventType}",
                            outboxEvent.Id, outboxEvent.EventType);
                    }
                    catch (Exception ex)
                    {
                        eventActivity?.SetTag("outbox.status", "error");
                        eventActivity?.SetTag("outbox.error", ex.Message);
                        
                        _logger.LogError(ex, "Failed to process outbox event {EventId}. Will retry later.", outboxEvent.Id);
                        await outboxRepository.IncrementRetryCountAsync(outboxEvent.Id);
                        
                        // Se excedeu tentativas, marca como falha permanente
                        if (await ShouldMarkAsFailed(outboxRepository, outboxEvent.Id))
                        {
                            await outboxRepository.MarkAsFailedAsync(outboxEvent.Id, ex.Message);
                            eventActivity?.SetTag("outbox.moved_to_dlq", true);
                            _logger.LogError("Outbox event {EventId} marked as permanently failed after max retries", outboxEvent.Id);
                        }
                    }
                }
                
                activity?.SetTag("outbox.batch_status", "completed");
            }
            catch (Exception ex)
            {
                activity?.SetTag("outbox.batch_status", "error");
                activity?.SetTag("outbox.batch_error", ex.Message);
                _logger.LogError(ex, "Error processing outbox events");
            }
        }

        private async Task<bool> ShouldMarkAsFailed(IOutboxRepository repository, string eventId)
        {
            const int maxRetries = 5;
            var events = await repository.GetUnprocessedEventsAsync();
            var currentEvent = events.FirstOrDefault(e => e.Id == eventId);
            
            if (currentEvent?.RetryCount >= maxRetries)
            {
                // Move para Dead Letter Queue ao invés de marcar como falha
                await repository.MarkAsDeadLetterAsync(eventId, 
                    $"Exceeded maximum retry attempts ({maxRetries})");
                return false; // Não marca como falha, move para DLQ
            }
            
            return false;
        }

        private async Task PublishEvent(Domain.Entities.OutboxEvent outboxEvent, IPublishEndpoint publishEndpoint)
        {
            using var activity = ActivitySource.StartActivity("outbox.publish_event");
            activity?.SetTag("outbox.event_id", outboxEvent.Id);
            activity?.SetTag("outbox.event_type", outboxEvent.EventType);
            
            switch (outboxEvent.EventType)
            {
                case nameof(OrderCreatedMessage):
                    var orderCreated = JsonSerializer.Deserialize<OrderCreatedMessage>(outboxEvent.EventData);
                    if (orderCreated != null)
                    {
                        activity?.SetTag("order.id", orderCreated.OrderId);
                        await publishEndpoint.Publish(orderCreated);
                    }
                    break;

                case nameof(OrderStatusUpdatedMessage):
                    var statusUpdated = JsonSerializer.Deserialize<OrderStatusUpdatedMessage>(outboxEvent.EventData);
                    if (statusUpdated != null)
                    {
                        activity?.SetTag("order.id", statusUpdated.OrderId);
                        await publishEndpoint.Publish(statusUpdated);
                    }
                    break;

                case nameof(OrderAcceptedMessage):
                    var orderAccepted = JsonSerializer.Deserialize<OrderAcceptedMessage>(outboxEvent.EventData);
                    if (orderAccepted != null)
                    {
                        activity?.SetTag("order.id", orderAccepted.OrderId);
                        activity?.SetTag("order.accepted_by", orderAccepted.UpdatedByUser);
                        await publishEndpoint.Publish(orderAccepted);
                    }
                    break;

                default:
                    activity?.SetTag("outbox.unknown_event", true);
                    _logger.LogWarning("Unknown event type: {EventType}", outboxEvent.EventType);
                    break;
            }
            
            activity?.SetTag("outbox.publish_status", "success");
        }

        public void Dispose()
        {
            _timer?.Dispose();
            ActivitySource?.Dispose();
        }
    }
}
