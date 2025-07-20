using FastTechFoodsOrder.Application.DTOs;
using FastTechFoodsOrder.Application.Interfaces;
using FastTechFoodsOrder.Domain.Entities;
using FastTechFoodsOrder.Shared.Enums;
using FastTechFoodsOrder.Shared.Integration.Messages;
using FastTechFoodsOrder.Shared.Utils;
using System.Diagnostics;
using System.Text.Json;

namespace FastTechFoodsOrder.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOutboxRepository _outboxRepository;
        private readonly IUnitOfWork _unitOfWork;
        
        // OpenTelemetry ActivitySource para tracing
        private static readonly ActivitySource ActivitySource = new("FastTechFoodsOrder.OrderService");

        public OrderService(
            IOrderRepository orderRepository,
            IOutboxRepository outboxRepository,
            IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _outboxRepository = outboxRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersAsync(string? customerId = null)
        {
            var orders = await _orderRepository.GetOrdersAsync(customerId);
            return orders.Select(MapToDto).OrderByDescending(x => x.OrderDate);
        }

        public async Task<OrderDto?> GetOrderByIdAsync(string? id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            var order = await _orderRepository.GetOrderByIdAsync(id);
            return order == null ? null : MapToDto(order);
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto)
        {
            //using var activity = ActivitySource.StartActivity("order.create");
            //activity?.SetTag("customer.id", dto.CustomerId);
            //activity?.SetTag("order.delivery_method", dto.DeliveryMethod);
            //activity?.SetTag("order.items_count", dto.Items.Count);
            
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var order = new Order
                {
                    CustomerId = dto.CustomerId,
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatusUtils.ConvertStatusToString(OrderStatus.Pending),
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
                            Status = OrderStatusUtils.ConvertStatusToString(OrderStatus.Pending),
                            StatusDate = DateTime.UtcNow,
                            UpdatedBy = "system"
                        }
                    }
                };

                // Calculate Total
                order.Total = order.Items.Sum(i => i.UnitPrice * i.Quantity);
                //activity?.SetTag("order.total", order.Total);

                var createdOrder = await _orderRepository.CreateOrderAsync(order);
                //activity?.SetTag("order.id", createdOrder.Id);

                // Add outbox event with trace context
                var orderCreatedEvent = new OrderCreatedMessage
                {
                    OrderId = createdOrder.Id,
                    EventType = "OrderCreated", 
                    EventDate = DateTime.UtcNow,
                    CustomerId = createdOrder.CustomerId,
                    Status = createdOrder.Status,
                    Items = createdOrder.Items.Select(i => new OrderItemMessage
                    {
                        ProductId = i.ProductId,
                        Name = i.Name,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice
                    }).ToList(),
                    Total = createdOrder.Total,
                    DeliveryMethod = createdOrder.DeliveryMethod,
                };

                var outboxEvent = new OutboxEvent
                {
                    EventType = nameof(OrderCreatedMessage),
                    EventData = JsonSerializer.Serialize(orderCreatedEvent),
                    CreatedAt = DateTime.UtcNow,
                    AggregateId = createdOrder.Id,
                    CorrelationId = Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString() // Propaga TraceId!
                };

                await _outboxRepository.AddEventAsync(outboxEvent);
                //activity?.SetTag("outbox.event_id", outboxEvent.Id);
                //activity?.SetTag("order.status", "created");

                return MapToDto(createdOrder);
            });
        }

        public async Task<bool> UpdateOrderStatusAsync(string id, UpdateOrderStatusDto dto)
        {
            //using var activity = ActivitySource.StartActivity("order.update_status");
            //activity?.SetTag("order.id", id);
            //activity?.SetTag("order.new_status", dto.Status);
            //activity?.SetTag("order.updated_by", dto.UpdatedBy);
            
            return await _unitOfWork.ExecuteInTransactionAsync(async (session) =>
            {
                // Busca o pedido atual para obter o status anterior
                var currentOrder = await _orderRepository.GetOrderByIdAsync(id);
                if (currentOrder == null)
                {
                    //activity?.SetTag("order.found", false);
                    return false;
                }

                var previousStatus = currentOrder.Status;
                //activity?.SetTag("order.previous_status", previousStatus);

                // 1. Atualiza o pedido
                var success = await _orderRepository.UpdateOrderStatusAsync(id, dto.Status, dto.UpdatedBy, session, dto.CancelReason);
                
                if (success)
                {
                    // 2. Cria evento para outbox
                    //var orderEvent = new OutboxEvent
                    //{
                    //    EventType = nameof(OrderCreatedMessage),
                    //    EventData = JsonSerializer.Serialize(new OrderCreatedMessage
                    //    {
                    //        OrderId = id,
                    //        EventType = "OrderStatusChanged",
                    //        EventDate = DateTime.UtcNow,
                    //        CustomerId = currentOrder.CustomerId,
                    //        Status = dto.Status,
                    //        PreviousStatus = previousStatus,
                    //        UpdatedBy = dto.UpdatedBy,
                    //    }),
                    //    CreatedAt = DateTime.UtcNow,
                    //    AggregateId = id,
                    //    CorrelationId = Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString() // Propaga TraceId!
                    //};
                    
                    // 3. Salva evento (na mesma transação!)
                    //await _outboxRepository.AddEventAsync(orderEvent, session);
                    //activity?.SetTag("outbox.event_id", orderEvent.Id);
                    //activity?.SetTag("order.status_updated", true);
                }
                
                return success;
            });
        }

        /// <summary>
        /// Atualiza apenas o status do pedido no banco de dados SEM criar eventos no Outbox.
        /// Use este método APENAS em consumers para evitar loops infinitos.
        /// </summary>
        public async Task<bool> UpdateOrderStatusDirectAsync(string id, UpdateOrderStatusDto dto)
        {
            using var activity = ActivitySource.StartActivity("order.update_status_direct");
            activity?.SetTag("order.id", id);
            activity?.SetTag("order.new_status", dto.Status);
            activity?.SetTag("order.updated_by", dto.UpdatedBy);
            activity?.SetTag("order.source", "consumer_direct_update");
            
            return await _unitOfWork.ExecuteInTransactionAsync(async (session) =>
            {
                // Busca o pedido atual para validação
                var currentOrder = await _orderRepository.GetOrderByIdAsync(id);
                if (currentOrder == null)
                {
                    activity?.SetTag("order.found", false);
                    return false;
                }

                var previousStatus = currentOrder.Status;
                activity?.SetTag("order.previous_status", previousStatus);

                // Atualiza APENAS o pedido no banco - SEM criar eventos no Outbox
                var success = await _orderRepository.UpdateOrderStatusAsync(id, dto.Status, dto.UpdatedBy, dto.CancelReason);
                
                activity?.SetTag("order.status_updated", success);
                return success;
            });
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
                }).ToList() ?? new List<OrderItemDto>(),
                StatusHistory = order.StatusHistory?.Select(s => new OrderStatusHistoryDto
                {
                    Status = s.Status,
                    StatusDate = s.StatusDate,
                    UpdatedBy = s.UpdatedBy
                }).ToList() ?? new List<OrderStatusHistoryDto>()
            };
        }
    }
}