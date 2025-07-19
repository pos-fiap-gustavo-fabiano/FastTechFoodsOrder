using FastTechFoodsOrder.Domain.Entities;
using MongoDB.Driver;

namespace FastTechFoodsOrder.Application.Interfaces
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetOrdersAsync(string? customerId = null);
        Task<Order?> GetOrderByIdAsync(string id);
        Task<Order> CreateOrderAsync(Order order);
        Task<bool> UpdateOrderStatusAsync(string id, string status, string updatedBy, string? cancelReason = null);
        
        // Novos métodos com suporte a sessão transacional
        Task<Order> CreateOrderAsync(Order order, IClientSessionHandle session);
        Task<bool> UpdateOrderStatusAsync(string id, string status, string updatedBy, IClientSessionHandle session, string? cancelReason = null);
    }
}
