using FastTechFoodsOrder.Application.DTOs;
using FastTechFoodsOrder.Application.Interfaces;
using FastTechFoodsOrder.Shared.Messages;
using MassTransit;

namespace FastTechFoodsOrder.Api.Consumers
{
    public class OrderPreparingConsumer : IConsumer<OrderPreparingMessage>
    {
        private readonly ILogger<OrderPreparingConsumer> _logger;
        private readonly IOrderService _orderService;

        public OrderPreparingConsumer(ILogger<OrderPreparingConsumer> logger, IOrderService orderService)
        {
            _logger = logger;
            _orderService = orderService;
        }

        public async Task Consume(ConsumeContext<OrderPreparingMessage> context)
        {
            var message = context.Message;
            _logger.LogInformation("Order is PREPARING - OrderId: {OrderId}, Customer: {CustomerId}",
            message.OrderId, message.UpdatedByUser);
            var id = message.OrderId;
            // Atualiza o status do pedido para "preparing"
            var dto = new UpdateOrderStatusDto
            {
                Status = "preparing",
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
