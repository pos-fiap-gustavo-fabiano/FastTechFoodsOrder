using System.Diagnostics;
using FastTechFoodsOrder.Api.Interfaces;
using FastTechFoodsOrder.Application.Interfaces;

namespace FastTechFoodsOrder.Api.Consumers
{
    //public class OrderStatusUpdatedConsumer : IMessageHandler<OrderStatusUpdatedMessage>
    //{
    //    private readonly ILogger<OrderStatusUpdatedConsumer> _logger;
    //    private readonly IOrderService _orderService;

    //    public OrderStatusUpdatedConsumer(ILogger<OrderStatusUpdatedConsumer> logger, IOrderService orderService)
    //    {
    //        _logger = logger;
    //        _orderService = orderService;
    //    }

    //    public Task HandleAsync(OrderStatusUpdatedMessage message, Activity? activity = null)
    //    {
    //        using var childActivity = activity?.Source.StartActivity("OrderStatusUpdatedConsumer.HandleAsync");
    //        childActivity?.SetTag("order.id", message.OrderId);
    //        childActivity?.SetTag("message.type", "OrderStatusUpdated");
            
    //        _logger.LogInformation("Order status updated - OrderId: {OrderId}, UpdatedBy: {UpdatedBy}",
    //            message.OrderId, message.UpdatedByUser);

    //        // Aqui você pode implementar a lógica específica para cada atualização de status
    //        // Por exemplo: enviar notificações, atualizar outros sistemas, etc.

    //        childActivity?.SetTag("operation.success", true);
    //        return Task.CompletedTask;
    //    }
    //}
}
