using FastTechFoodsOrder.Application.DTOs;
using FastTechFoodsOrder.Application.Interfaces;
using FastTechFoodsOrder.Domain.Entities;

namespace FastTechFoodsOrder.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersAsync(string customerId = null)
        {
            var orders = await _orderRepository.GetOrdersAsync(customerId);
            return orders.Select(MapToDto).OrderByDescending(x => x.OrderDate);
        }

        public async Task<OrderDto> GetOrderByIdAsync(string id)
        {
            var order = await _orderRepository.GetOrderByIdAsync(id);
            return order == null ? null : MapToDto(order);
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto)
        {
            var order = new Order
            {
                CustomerId = dto.CustomerId,
                OrderDate = DateTime.UtcNow,
                Status = "pending",
                DeliveryMethod = dto.DeliveryMethod,
                Items = dto.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice, 
                    Name = i.Name
                }).ToList(),
                StatusHistory = new List<OrderStatusHistory>
                {
                    new OrderStatusHistory
                    {
                        Status = "pending",
                        StatusDate = DateTime.UtcNow,
                        UpdatedBy = "system"
                    }
                }
            };

            // Calculate Total (if you fetch UnitPrice from Product Service, do here)
            order.Total = order.Items.Sum(i => i.UnitPrice * i.Quantity);

            var createdOrder = await _orderRepository.CreateOrderAsync(order);
            return MapToDto(createdOrder);
        }

        public async Task<bool> UpdateOrderStatusAsync(string id, UpdateOrderStatusDto dto)
        {
            // Busca o pedido atual para obter o status anterior
            var currentOrder = await _orderRepository.GetOrderByIdAsync(id);
            if (currentOrder == null)
                return false;

            var previousStatus = currentOrder.Status;

            // Atualiza o status no repositório
            var success = await _orderRepository.UpdateOrderStatusAsync(id, dto.Status, dto.UpdatedBy, dto.CancelReason);

            //if (success)
            //{
            //    // Busca o pedido atualizado para publicar na mensagem
            //    var updatedOrder = await _orderRepository.GetOrderByIdAsync(id);
            //    if (updatedOrder != null)
            //    {
            //        var orderDto = MapToDto(updatedOrder);

            //        // Publica a mensagem de atualização de status
            //        await _messagePublisher.PublishOrderStatusUpdatedAsync(
            //            orderDto,
            //            previousStatus,
            //            dto.Status,
            //            dto.UpdatedBy,
            //            dto.CancelReason
            //        );
            //    }
            //}

            return success;
        }

        // --- Mapping methods ---
        private static OrderDto MapToDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                OrderDate = order.OrderDate,
                Status = order.Status,
                DeliveryMethod = order.DeliveryMethod,
                CancelReason = order.CancelReason,
                Total = order.Total,
                Items = order.Items?.Select(i => new OrderItemDto
                {
                    ProductId = i.ProductId,
                    Name = i.Name,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList(),
                StatusHistory = order.StatusHistory?.Select(s => new OrderStatusHistoryDto
                {
                    Status = s.Status,
                    StatusDate = s.StatusDate,
                    UpdatedBy = s.UpdatedBy
                }).ToList()
            };
        }
    }
}