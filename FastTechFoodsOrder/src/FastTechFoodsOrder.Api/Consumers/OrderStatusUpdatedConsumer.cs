using FastTechFoodsOrder.Application.Interfaces;
using FastTechFoodsOrder.Shared.Messages;
using MassTransit;

namespace FastTechFoodsOrder.Api.Consumers
{
    public class OrderStatusUpdatedConsumer : IConsumer<OrderStatusUpdatedMessage>
    {
        private readonly ILogger<OrderStatusUpdatedConsumer> _logger;
        private readonly IOrderService _orderService;

        public OrderStatusUpdatedConsumer(ILogger<OrderStatusUpdatedConsumer> logger, IOrderService orderService)
        {
            _logger = logger;
            _orderService = orderService;
        }

        public Task Consume(ConsumeContext<OrderStatusUpdatedMessage> context)
        {
            var message = context.Message;
            
            _logger.LogInformation("Order status updated - OrderId: {OrderId}, From: {PreviousStatus}, To: {NewStatus}, UpdatedBy: {UpdatedBy}",
                message.OrderId, message);

            // Aqui você pode implementar a lógica específica para cada atualização de status
            // Por exemplo: enviar notificações, atualizar outros sistemas, etc.

            return Task.CompletedTask;
        }
    }
}
