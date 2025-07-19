namespace FastTechFoodsOrder.Shared.Integration.Messages
{
    public class OrderEventMessage
    {
        public required string OrderId { get; set; }
        public string EventType { get; set; }
        public DateTime EventDate { get; set; }
        public string CustomerId { get; set; }
        public required string Status { get; set; }
    }

    // Para eventos que precisam dos itens (criação, modificação de itens)
    public class OrderCreatedMessage : OrderEventMessage
    {
        public required List<OrderItemMessage> Items { get; set; }
        public required string DeliveryMethod { get; set; }
        public decimal Total { get; set; }
    }

    // Para eventos simples de mudança de status
    //public class OrderStatusChangedMessage : OrderEventMessage
    //{
    //    public required string PreviousStatus { get; set; }
    //    public required string UpdatedBy { get; set; }
    //}

    // Status específicos - Pending
    public class OrderPendingMessage : OrderEventMessage
    {
        public required string UpdatedBy { get; set; }
        public string? Notes { get; set; }
    }

    // Status específicos - Accepted
    public class OrderAcceptedMessage : OrderEventMessage
    {
        public required string UpdatedBy { get; set; }
        public DateTime EstimatedPreparationTime { get; set; }
    }

    // Status específicos - Preparing
    public class OrderPreparingMessage : OrderEventMessage
    {
        public required string UpdatedBy { get; set; }
        public DateTime StartedPreparationAt { get; set; }
        public int EstimatedMinutes { get; set; }
    }

    // Status específicos - Ready
    public class OrderReadyMessage : OrderEventMessage
    {
        public required string UpdatedBy { get; set; }
        public DateTime ReadyAt { get; set; }
    }

    // Status específicos - Delivered
    public class OrderDeliveredMessage : OrderEventMessage
    {
        public required string UpdatedBy { get; set; }
        public DateTime DeliveredAt { get; set; }
        public string? DeliveredBy { get; set; }
        public string? DeliveryNotes { get; set; }
        public string? CustomerSignature { get; set; }
    }

    // Status específicos - Completed
    public class OrderCompletedMessage : OrderEventMessage
    {
        public required string PreviousStatus { get; set; }
        public required string UpdatedBy { get; set; }
        public DateTime CompletedAt { get; set; }
        public string? DeliveredBy { get; set; }
        public string? CompletionNotes { get; set; }
        public decimal FinalAmount { get; set; }
    }

    // Para cancelamentos
    public class OrderCancelledMessage : OrderEventMessage
    {
        public required string CancelReason { get; set; }
        public required string CancelledBy { get; set; }
        public DateTime CancelledAt { get; set; }
    }

    public class OrderItemMessage
    {
        public required string ProductId { get; set; }
        public required string Name { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
