using System.Diagnostics;
using FastTechFoodsOrder.Api.Interfaces;
using FastTechFoodsOrder.Shared.Integration.Messages;

namespace FastTechFoodsOrder.Api.Consumers
{
    public class OrderCompletedConsumer : IMessageHandler<OrderCompletedMessage>
    {
        private readonly ILogger<OrderCompletedConsumer> _logger;

        public OrderCompletedConsumer(ILogger<OrderCompletedConsumer> logger)
        {
            _logger = logger;
        }

        public Task HandleAsync(OrderCompletedMessage message, Activity? activity = null)
        {
            using var childActivity = activity?.Source.StartActivity("OrderCompletedConsumer.HandleAsync");
            childActivity?.SetTag("order.id", message.OrderId);
            childActivity?.SetTag("message.type", "OrderCompleted");

            _logger.LogInformation("Order COMPLETED - OrderId: {OrderId}, Customer: {CustomerId}",
                message.OrderId, message.UpdatedBy);

            childActivity?.SetTag("operation.success", true);
            return Task.CompletedTask;
        }
    }
}
