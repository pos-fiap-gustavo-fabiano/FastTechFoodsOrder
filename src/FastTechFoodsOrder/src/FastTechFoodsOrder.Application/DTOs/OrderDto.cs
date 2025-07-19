namespace FastTechFoodsOrder.Application.DTOs
{
    public class OrderDto
    {
        public string Id { get; set; }
        public string CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public string DeliveryMethod { get; set; }
        public string CancelReason { get; set; }
        public decimal Total { get; set; }
        public List<OrderItemDto> Items { get; set; }
        public List<OrderStatusHistoryDto> StatusHistory { get; set; }
    }

    public class OrderItemDto
    {
        public string ProductId { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class OrderStatusHistoryDto
    {
        public string Status { get; set; }
        public DateTime StatusDate { get; set; }
        public string UpdatedBy { get; set; }
    }
}
