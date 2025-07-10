using FastTechFoodsOrder.Application.DTOs;
using FastTechFoodsOrder.Application.Interfaces;
using FastTechFoodsOrder.Shared.Messages;
using MassTransit;

namespace FastTechFoodsOrder.Api.Consumers
{
    public class OrderReadyConsumer : IConsumer<OrderReadyMessage>
    {
        private readonly ILogger<OrderReadyConsumer> _logger;
        private readonly IOrderService _orderService;

        public OrderReadyConsumer(ILogger<OrderReadyConsumer> logger, IOrderService orderService)
        {
            _logger = logger;
            _orderService = orderService;
        }

        public async Task Consume(ConsumeContext<OrderReadyMessage> context)
        {
            var message = context.Message;
            _logger.LogInformation("Order is READY - OrderId: {OrderId}, Customer: {CustomerId}", 
                message.OrderId, message.UpdatedByUser);

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
