using System.Diagnostics;
using FastTechFoodsOrder.Api.Interfaces;

namespace FastTechFoodsOrder.Api.Consumers
{
    //public class OrderCreatedConsumer : IMessageHandler<OrderCreatedMessage>
    //{
    //    private readonly ILogger<OrderCreatedConsumer> _logger;

    //    public OrderCreatedConsumer(ILogger<OrderCreatedConsumer> logger)
    //    {
    //        _logger = logger;
    //    }

    //    public Task HandleAsync(OrderCreatedMessage message, Activity? activity = null)
    //    {
    //        using var childActivity = activity?.Source.StartActivity("OrderCreatedConsumer.HandleAsync");
    //        childActivity?.SetTag("order.id", message.OrderId);
    //        childActivity?.SetTag("message.type", "OrderCreated");

    //        _logger.LogInformation("Order CREATED - OrderId: {OrderId}, Customer: {CustomerId}", 
    //            message.OrderId, message.UpdatedByUser);
            
    //        // Lógica específica para pedidos criados
    //        // Ex: Validar disponibilidade, calcular preços, etc.
            
    //        childActivity?.SetTag("operation.success", true);
    //        return Task.CompletedTask;
    //    }
    //}
}
