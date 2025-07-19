namespace FastTechFoodsOrder.Application.DTOs
{
    public class CreateOrderDto
    {
        public string CustomerId { get; set; }
        public string DeliveryMethod { get; set; }
        public List<CreateOrderItemDto> Items { get; set; }
    }

    public class CreateOrderItemDto
    {
        public string ProductId { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
