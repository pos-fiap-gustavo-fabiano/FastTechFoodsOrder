using FastTechFoodsOrder.Application.DTOs;
using FastTechFoodsOrder.Application.Interfaces;
using FastTechFoodsOrder.Shared.Messages;
using MassTransit;

namespace FastTechFoodsOrder.Api.Consumers
{
    public class OrderAcceptedConsumer : IConsumer<OrderAcceptedMessage>
    {
        private readonly ILogger<OrderAcceptedConsumer> _logger;
        private readonly IOrderService _orderService;

        public OrderAcceptedConsumer(ILogger<OrderAcceptedConsumer> logger, IOrderService orderService)
        {
            _logger = logger;
            _orderService = orderService;
        }

        public async Task Consume(ConsumeContext<OrderAcceptedMessage> context)
        {
            var message = context.Message;
            _logger.LogInformation("Order ACCEPTED - OrderId: {OrderId}, Customer: {CustomerId}", 
                message.OrderId, message.UpdatedByUser);
            var id = message.OrderId;
            var dto = new UpdateOrderStatusDto
            {
                Status = "accepted",
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
