using FastTechFoodsOrder.Application.DTOs;

namespace FastTechFoodsOrder.Application.Interfaces
{
    public interface IOrderMessagePublisher
    {
        Task PublishOrderStatusUpdatedAsync(OrderDto order, string previousStatus, string newStatus, string updatedBy, string? cancelReason = null);
    }
}
