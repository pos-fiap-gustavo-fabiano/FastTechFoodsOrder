using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FastTechFoodsOrder.Domain.Entities
{
    public class Order
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } 
        public string DeliveryMethod { get; set; }
        public string CancelReason { get; set; }
        public decimal Total { get; set; }
        public List<OrderItem> Items { get; set; }
        public List<OrderStatusHistory> StatusHistory { get; set; }
    }

    public class OrderItem
    {
        public string ProductId { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class OrderStatusHistory
    {
        public string Status { get; set; }
        public DateTime StatusDate { get; set; }
        public string UpdatedBy { get; set; }
    }
}
