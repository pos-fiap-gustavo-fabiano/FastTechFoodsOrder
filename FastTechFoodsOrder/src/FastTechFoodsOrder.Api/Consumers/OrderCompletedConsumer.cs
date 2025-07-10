using FastTechFoodsOrder.Shared.Messages;
using MassTransit;

namespace FastTechFoodsOrder.Api.Consumers
{
    public class OrderCompletedConsumer : IConsumer<OrderCompletedMessage>
    {
        private readonly ILogger<OrderCompletedConsumer> _logger;

        public OrderCompletedConsumer(ILogger<OrderCompletedConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<OrderCompletedMessage> context)
        {
            var message = context.Message;
            _logger.LogInformation("Order COMPLETED - OrderId: {OrderId}, Customer: {CustomerId}", 
                message.OrderId, message.UpdatedByUser);
            
            return Task.CompletedTask;
        }
    }
}
