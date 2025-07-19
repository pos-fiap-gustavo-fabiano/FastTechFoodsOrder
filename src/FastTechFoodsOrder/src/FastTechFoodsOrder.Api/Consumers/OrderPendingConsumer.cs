using System.Diagnostics;
using FastTechFoodsOrder.Api.Interfaces;

namespace FastTechFoodsOrder.Api.Consumers
{
    //public class OrderPendingConsumer : IMessageHandler<OrderPendingMessage>
    //{
    //    private readonly ILogger<OrderPendingConsumer> _logger;

    //    public OrderPendingConsumer(ILogger<OrderPendingConsumer> logger)
    //    {
    //        _logger = logger;
    //    }

    //    public Task HandleAsync(OrderPendingMessage message, Activity? activity = null)
    //    {
    //        using var childActivity = activity?.Source.StartActivity("OrderPendingConsumer.HandleAsync");
    //        childActivity?.SetTag("order.id", message.OrderId);
    //        childActivity?.SetTag("message.type", "OrderPending");

    //        _logger.LogInformation("Order is now PENDING - OrderId: {OrderId}, Customer: {CustomerId}", 
    //            message.OrderId, message.UpdatedByUser);
            
    //        // Lógica específica para pedidos pendentes
    //        // Ex: Notificar cozinha, validar estoque, etc.
            
    //        childActivity?.SetTag("operation.success", true);
    //        return Task.CompletedTask;
    //    }
    //}
}
