namespace FastTechFoodsOrder.Shared.Messages
{
    public class OrderStatusUpdatedMessage
    {
        public string OrderId { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string UpdatedByUser { get; set; } = string.Empty;
        public int EstimatedPreparationTime { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public class OrderPendingMessage : OrderStatusUpdatedMessage { }
    public class OrderAcceptedMessage : OrderStatusUpdatedMessage { }
    public class OrderCreatedMessage : OrderStatusUpdatedMessage { }
    public class OrderPreparingMessage : OrderStatusUpdatedMessage { }
    public class OrderReadyMessage : OrderStatusUpdatedMessage { }
    public class OrderCompletedMessage : OrderStatusUpdatedMessage { }
    public class OrderCancelledMessage : OrderStatusUpdatedMessage { }
}
