using FastTechFoodsOrder.Application.DTOs;
using FastTechFoodsOrder.Application.Interfaces;
using FastTechFoodsOrder.Shared.Messages;
using MassTransit;

namespace FastTechFoodsOrder.Application.Services
{
    public class OrderMessagePublisher : IOrderMessagePublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public OrderMessagePublisher(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task PublishOrderCreatedAsync(OrderDto order)
        {
            var message = new OrderCreatedMessage
            {
                OrderId = order.Id,
            };

            await _publishEndpoint.Publish(message);
        }

        public async Task PublishOrderStatusUpdatedAsync(
            OrderDto order, 
            string previousStatus, 
            string newStatus, 
            string updatedBy, 
            string? cancelReason = null)
        {
            var message = new OrderStatusUpdatedMessage
            {
                OrderId = order.Id,
                UpdatedAt = DateTime.UtcNow,
            };

            await _publishEndpoint.Publish(message);
        }

        public async Task PublishOrderAcceptedAsync(string orderId, string customerId, string acceptedBy)
        {
            var message = new OrderAcceptedMessage
            {
                OrderId = orderId,
            };

            await _publishEndpoint.Publish(message);
        }
    }
}
