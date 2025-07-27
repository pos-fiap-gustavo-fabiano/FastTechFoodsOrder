using System.Diagnostics;
using FastTechFoodsOrder.Api.Interfaces;
using FastTechFoodsOrder.Application.DTOs;
using FastTechFoodsOrder.Application.Interfaces;
using FastTechFoodsOrder.Shared.Enums;
using FastTechFoodsOrder.Shared.Integration.Messages;
using FastTechFoodsOrder.Shared.Utils;

namespace FastTechFoodsOrder.Api.Consumers
{
    public class OrderReadyConsumer : IMessageHandler<OrderReadyMessage>
    {
        private readonly ILogger<OrderReadyConsumer> _logger;
        private readonly IOrderService _orderService;

        public OrderReadyConsumer(ILogger<OrderReadyConsumer> logger, IOrderService orderService)
        {
            _logger = logger;
            _orderService = orderService;
        }

        public async Task HandleAsync(OrderReadyMessage message, Activity? activity = null)
        {
            using var childActivity = activity?.Source.StartActivity("OrderReadyConsumer.HandleAsync");
            childActivity?.SetTag("order.id", message.OrderId);
            childActivity?.SetTag("message.type", "OrderReady");

            _logger.LogInformation("Order is READY - OrderId: {OrderId}, Customer: {CustomerId}",
                message.OrderId, message.UpdatedBy);

            var id = message.OrderId;
            var dto = new UpdateOrderStatusDto
            {
                Status = OrderStatusUtils.ConvertStatusToString(OrderStatus.Ready),
                UpdatedBy = message.UpdatedBy,
            };
            var result = await _orderService.UpdateOrderStatusDirectAsync(id, dto);

            if (result.IsFailure)
            {
                _logger.LogError("Failed to update order status to READY for OrderId: {OrderId}. Error: {Error}", 
                    message.OrderId, result.ErrorMessage);
                childActivity?.SetTag("operation.success", false);
                throw new Exception($"Failed to update order status to READY for OrderId: {message.OrderId}. Error: {result.ErrorMessage}");
            }

            childActivity?.SetTag("operation.success", true);
        }
    }
}
