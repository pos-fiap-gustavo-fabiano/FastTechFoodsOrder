using FastTechFoodsOrder.Application.DTOs;

namespace FastTechFoodsOrder.Application.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDto>> GetOrdersAsync(string? customerId = null);
        Task<OrderDto?> GetOrderByIdAsync(string? id);
        Task<OrderDto> CreateOrderAsync(CreateOrderDto dto);
        Task<bool> UpdateOrderStatusAsync(string id, UpdateOrderStatusDto dto);
        
        /// <summary>
        /// Atualiza apenas o status do pedido no banco de dados SEM criar eventos no Outbox.
        /// Use este método APENAS em consumers para evitar loops infinitos.
        /// </summary>
        Task<bool> UpdateOrderStatusDirectAsync(string id, UpdateOrderStatusDto dto);
    }
}
