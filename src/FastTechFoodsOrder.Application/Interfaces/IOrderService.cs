using FastTechFoodsOrder.Application.DTOs;
using FastTechFoodsOrder.Shared.Results;

namespace FastTechFoodsOrder.Application.Interfaces
{
    public interface IOrderService
    {
        Task<Result<IEnumerable<OrderDto>>> GetOrdersAsync(string? customerId = null);
        Task<Result<OrderDto>> GetOrderByIdAsync(string? id);
        Task<Result<OrderDto>> CreateOrderAsync(CreateOrderDto dto);
        Task<Result> UpdateOrderStatusAsync(string id, UpdateOrderStatusDto dto);
        Task<Result> CancelOrderAsync(string id, UpdateOrderStatusDto dto);
        Task<Result> UpdateOrderStatusDirectAsync(string id, UpdateOrderStatusDto dto);
    }
}
