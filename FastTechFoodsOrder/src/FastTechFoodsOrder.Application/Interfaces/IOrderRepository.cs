using FastTechFoodsOrder.Domain.Entities;

namespace FastTechFoodsOrder.Application.Interfaces
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetOrdersAsync(string customerId = null);
        Task<Order> GetOrderByIdAsync(string id);
        Task<Order> CreateOrderAsync(Order order);
        Task<bool> UpdateOrderStatusAsync(string id, string status, string updatedBy, string cancelReason = null);
    }
}
