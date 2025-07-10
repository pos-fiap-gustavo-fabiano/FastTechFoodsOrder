using FastTechFoodsOrder.Application.DTOs;
using FastTechFoodsOrder.Application.Interfaces;
using FastTechFoodsOrder.Shared.Messages;
using MassTransit;

namespace FastTechFoodsOrder.Api.Consumers
{
    public class OrderCancelledConsumer : IConsumer<OrderCancelledMessage>
    {
        private readonly ILogger<OrderCancelledConsumer> _logger;
        private readonly IOrderService _orderService;

        public OrderCancelledConsumer(ILogger<OrderCancelledConsumer> logger, IOrderService orderService)
        {
            _logger = logger;
            _orderService = orderService;
        }

        public async Task Consume(ConsumeContext<OrderCancelledMessage> context)
        {
            var message = context.Message;
            _logger.LogInformation("Order CANCELLED - OrderId: {OrderId}, Customer: {CustomerId}, Reason: {CancelReason}", 
                message.OrderId, message.UpdatedByUser, message.Notes);

            var id = message.OrderId;
            var dto = new UpdateOrderStatusDto
            {
                Status = "ready",
                UpdatedBy = message.UpdatedByUser,
            };
            var updated = await _orderService.UpdateOrderStatusAsync(id, dto);

            if (!updated)
            {
                _logger.LogError("Failed to update order status to ACCEPTED for OrderId: {OrderId}", message.OrderId);
                return;
            }
        }
    }
}
