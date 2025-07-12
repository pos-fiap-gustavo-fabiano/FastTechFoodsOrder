using FastTechFoodsOrder.Application.DTOs;

namespace FastTechFoodsOrder.Application.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDto>> GetOrdersAsync(string? customerId = null);
        Task<OrderDto?> GetOrderByIdAsync(string? id);
        Task<OrderDto> CreateOrderAsync(CreateOrderDto dto);
        Task<bool> UpdateOrderStatusAsync(string id, UpdateOrderStatusDto dto);
    }
}
