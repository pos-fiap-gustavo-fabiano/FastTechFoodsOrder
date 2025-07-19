using System.Diagnostics;
using FastTechFoodsOrder.Api.Interfaces;
using FastTechFoodsOrder.Application.DTOs;
using FastTechFoodsOrder.Application.Interfaces;
using FastTechFoodsOrder.Shared.Integration.Messages;

namespace FastTechFoodsOrder.Api.Consumers
{
    public class OrderAcceptedConsumer : IMessageHandler<OrderAcceptedMessage>
    {
        private readonly ILogger<OrderAcceptedConsumer> _logger;
        private readonly IOrderService _orderService;

        public OrderAcceptedConsumer(ILogger<OrderAcceptedConsumer> logger, IOrderService orderService)
        {
            _logger = logger;
            _orderService = orderService;
        }

        public async Task HandleAsync(OrderAcceptedMessage message, Activity? activity = null)
        {
            using var childActivity = activity?.Source.StartActivity("OrderAcceptedConsumer.HandleAsync");
            childActivity?.SetTag("order.id", message.OrderId);
            childActivity?.SetTag("message.type", "OrderAccepted");

            _logger.LogInformation("Order ACCEPTED - OrderId: {OrderId}, UpdatedBy: {UpdatedBy}", 
                message.OrderId, message.UpdatedBy);
            
            // Atualiza o status no banco usando o método que NÃO cria nova mensagem no Outbox
            var dto = new UpdateOrderStatusDto
            {
                Status = "accepted",
                UpdatedBy = message.UpdatedBy,
            };
            
            // Usa UpdateOrderStatusDirectAsync para evitar loop infinito
            var updated = await _orderService.UpdateOrderStatusDirectAsync(message.OrderId, dto);
            
            if (!updated)
            {
                _logger.LogError("Failed to update order status to ACCEPTED for OrderId: {OrderId}", message.OrderId);
                childActivity?.SetTag("operation.success", false);
                throw new Exception($"Failed to update order status to ACCEPTED for OrderId: {message.OrderId}");
            }
            
            // Aqui você pode adicionar outras lógicas de negócio para quando um pedido é aceito
            // Por exemplo: notificar cozinha, validar estoque, etc.
            
            _logger.LogInformation("Order acceptance processing completed for OrderId: {OrderId}", message.OrderId);
            childActivity?.SetTag("operation.success", true);
        }
    }
}
