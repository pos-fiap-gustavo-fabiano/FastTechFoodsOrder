using FastTechFoodsOrder.Application.Interfaces;
using FastTechFoodsOrder.Domain.Entities;
using FastTechFoodsOrder.Infra.Context;
using MongoDB.Driver;

namespace FastTechFoodsOrder.Infra.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IMongoCollection<Order> _orders;

        public OrderRepository(ApplicationDbContext context)
        {
            _orders = context.GetCollection<Order>("orders");
        }

        public async Task<IEnumerable<Order>> GetOrdersAsync(string? customerId = null)
        {
            if (!string.IsNullOrEmpty(customerId))
                return await _orders.Find(x => x.CustomerId == customerId).ToListAsync();
            return await _orders.Find(_ => true).ToListAsync();
        }

        public async Task<Order?> GetOrderByIdAsync(string id)
        {
            return await _orders.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            await _orders.InsertOneAsync(order);
            return order;
        }

        public async Task<Order> CreateOrderAsync(Order order, IClientSessionHandle session)
        {
            await _orders.InsertOneAsync(session, order);
            return order;
        }

        public async Task<bool> UpdateOrderStatusAsync(string id, string status, string updatedBy, string? cancelReason = null)
        {
            var update = Builders<Order>.Update
                .Set(x => x.Status, status)
                .Push(x => x.StatusHistory, new OrderStatusHistory
                {
                    Status = status,
                    StatusDate = DateTime.UtcNow,
                    UpdatedBy = updatedBy
                })
                .Set(x => x.CancelReason, cancelReason);

            var result = await _orders.UpdateOneAsync(x => x.Id == id, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateOrderStatusAsync(string id, string status, string updatedBy, IClientSessionHandle session, string? cancelReason = null)
        {
            var update = Builders<Order>.Update
                .Set(x => x.Status, status)
                .Push(x => x.StatusHistory, new OrderStatusHistory
                {
                    Status = status,
                    StatusDate = DateTime.UtcNow,
                    UpdatedBy = updatedBy
                })
                .Set(x => x.CancelReason, cancelReason);

            var result = await _orders.UpdateOneAsync(session, x => x.Id == id, update);
            return result.ModifiedCount > 0;
        }
    }
}
