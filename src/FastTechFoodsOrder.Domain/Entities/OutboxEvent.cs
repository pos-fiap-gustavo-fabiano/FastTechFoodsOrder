using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FastTechFoodsOrder.Domain.Entities
{
    public class OutboxEvent
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        public string EventType { get; set; }
        public string EventData { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime? NextRetryAt { get; set; } // Nova propriedade para backoff
        public bool IsProcessed { get; set; }
        public int RetryCount { get; set; }
        public string? ErrorMessage { get; set; }
        public string AggregateId { get; set; } // OrderId
        public string CorrelationId { get; set; }

        // Adicionar propriedades para Dead Letter Queue
        public string? DeadLetterReason { get; set; }
        public DateTime? DeadLetterAt { get; set; }
        public bool IsDeadLetter { get; set; }
    }
}
