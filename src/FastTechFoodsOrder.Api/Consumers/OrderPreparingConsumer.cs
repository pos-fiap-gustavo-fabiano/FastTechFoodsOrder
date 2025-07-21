using System.Diagnostics;
using FastTechFoodsOrder.Api.Interfaces;
using FastTechFoodsOrder.Application.DTOs;
using FastTechFoodsOrder.Application.Interfaces;
using FastTechFoodsOrder.Shared.Enums;
using FastTechFoodsOrder.Shared.Integration.Messages;
using FastTechFoodsOrder.Shared.Utils;

namespace FastTechFoodsOrder.Api.Consumers
{
    public class OrderPreparingConsumer : IMessageHandler<OrderPreparingMessage>
    {
        private readonly ILogger<OrderPreparingConsumer> _logger;
        private readonly IOrderService _orderService;

        public OrderPreparingConsumer(ILogger<OrderPreparingConsumer> logger, IOrderService orderService)
        {
            _logger = logger;
            _orderService = orderService;
        }

        public async Task HandleAsync(OrderPreparingMessage message, Activity? activity = null)
        {
            using var childActivity = activity?.Source.StartActivity("OrderPreparingConsumer.HandleAsync");
            childActivity?.SetTag("order.id", message.OrderId);
            childActivity?.SetTag("message.type", "OrderPreparing");

            _logger.LogInformation("Order is PREPARING - OrderId: {OrderId}, Customer: {CustomerId}",
                message.OrderId, message.UpdatedBy);

            var id = message.OrderId;
            var dto = new UpdateOrderStatusDto
            {
                Status = OrderStatusUtils.ConvertStatusToString(OrderStatus.Preparing),
                UpdatedBy = message.UpdatedBy,
            };
            var updated = await _orderService.UpdateOrderStatusDirectAsync(id, dto);

            if (!updated)
            {
                _logger.LogError("Failed to update order status to PREPARING for OrderId: {OrderId}", message.OrderId);
                childActivity?.SetTag("operation.success", false);
                throw new Exception($"Failed to update order status to PREPARING for OrderId: {message.OrderId}");
            }

            childActivity?.SetTag("operation.success", true);
        }
    }
}
