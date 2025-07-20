using System.Diagnostics;
using FastTechFoodsOrder.Api.Interfaces;
using FastTechFoodsOrder.Application.DTOs;
using FastTechFoodsOrder.Application.Interfaces;
using FastTechFoodsOrder.Shared.Integration.Messages;
using FastTechFoodsOrder.Shared.Utils;

namespace FastTechFoodsOrder.Api.Consumers
{
    public class OrderCancelledConsumer : IMessageHandler<OrderCancelledMessage>
    {
        private readonly ILogger<OrderCancelledConsumer> _logger;
        private readonly IOrderService _orderService;

        public OrderCancelledConsumer(ILogger<OrderCancelledConsumer> logger, IOrderService orderService)
        {
            _logger = logger;
            _orderService = orderService;
        }

        public async Task HandleAsync(OrderCancelledMessage message, Activity? activity = null)
        {
            try
            {
                using var childActivity = activity?.Source.StartActivity("OrderCancelledConsumer.HandleAsync");
                childActivity?.SetTag("order.id", message.OrderId);
                childActivity?.SetTag("message.type", "OrderCancelled");

                _logger.LogInformation("Order CANCELLED - OrderId: {OrderId}, Customer: {CustomerId}, Reason: {CancelReason}",
                    message.OrderId, message.CancelledBy, message.CancelReason);

                var id = message.OrderId;
                var dto = new UpdateOrderStatusDto
                {
                    Status = OrderStatusUtils.ConvertStatusToString(Shared.Enums.OrderStatus.Cancelled),
                    UpdatedBy = message.CancelledBy,
                };
                var updated = await _orderService.UpdateOrderStatusDirectAsync(id, dto);

                if (!updated)
                {
                    _logger.LogError("Failed to update order status to CANCELLED for OrderId: {OrderId}", message.OrderId);
                    childActivity?.SetTag("operation.success", false);
                    throw new Exception($"Failed to update order status to CANCELLED for OrderId: {message.OrderId}");
                }

                childActivity?.SetTag("operation.success", true);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error processing OrderCancelledMessage for OrderId: {OrderId}", message.OrderId);
                throw ex;
            }
        }
    }
}
