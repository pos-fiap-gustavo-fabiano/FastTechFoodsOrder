using FastTechFoodsOrder.Shared.Messages;
using MassTransit;

namespace FastTechFoodsOrder.Api.Consumers
{
    public class OrderPendingConsumer : IConsumer<OrderPendingMessage>
    {
        private readonly ILogger<OrderPendingConsumer> _logger;

        public OrderPendingConsumer(ILogger<OrderPendingConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<OrderPendingMessage> context)
        {
            var message = context.Message;
            _logger.LogInformation("Order is now PENDING - OrderId: {OrderId}, Customer: {CustomerId}", 
                message.OrderId, message.UpdatedByUser);
            
            // Lógica específica para pedidos pendentes
            // Ex: Notificar cozinha, validar estoque, etc.
            
            return Task.CompletedTask;
        }
    }
}
