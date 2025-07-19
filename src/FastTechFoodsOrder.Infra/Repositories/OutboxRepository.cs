using FastTechFoodsOrder.Application.Interfaces;
using FastTechFoodsOrder.Domain.Entities;
using FastTechFoodsOrder.Infra.Context;
using MongoDB.Driver;

namespace FastTechFoodsOrder.Infra.Repositories
{
    public class OutboxRepository : IOutboxRepository
    {
        private readonly IMongoCollection<OutboxEvent> _outboxEvents;

        public OutboxRepository(ApplicationDbContext context)
        {
            _outboxEvents = context.GetCollection<OutboxEvent>("outbox_events");
        }

        public async Task AddEventAsync(OutboxEvent outboxEvent)
        {
            await _outboxEvents.InsertOneAsync(outboxEvent);
        }

        public async Task AddEventAsync(OutboxEvent outboxEvent, IClientSessionHandle session)
        {
            await _outboxEvents.InsertOneAsync(session, outboxEvent);
        }

        public async Task<IEnumerable<OutboxEvent>> GetUnprocessedEventsAsync(int batchSize = 50)
        {
            var now = DateTime.UtcNow;
            
            // Busca eventos prontos para processamento (sem NextRetryAt ou já passou da data)
            return await _outboxEvents
                .Find(x => !x.IsProcessed && !x.IsDeadLetter && 
                          (x.NextRetryAt == null || x.NextRetryAt <= now))
                .Sort(Builders<OutboxEvent>.Sort.Ascending(x => x.CreatedAt))
                .Limit(batchSize)
                .ToListAsync();
        }

        public async Task IncrementRetryCountAsync(string eventId)
        {
            var now = DateTime.UtcNow;
            var currentEvent = await _outboxEvents.Find(x => x.Id == eventId).FirstOrDefaultAsync();
            if (currentEvent == null) return;

            var newRetryCount = currentEvent.RetryCount + 1;
            var backoffMinutes = CalculateBackoffMinutes(newRetryCount);
            
            var update = Builders<OutboxEvent>.Update
                .Inc(x => x.RetryCount, 1)
                .Set(x => x.NextRetryAt, now.AddMinutes(backoffMinutes));

            await _outboxEvents.UpdateOneAsync(x => x.Id == eventId, update);
        }

        private static int CalculateBackoffMinutes(int retryCount)
        {
            // Backoff exponencial: 1min, 2min, 4min, 8min, 16min (máximo)
            return (int)Math.Pow(2, Math.Min(retryCount, 5));
        }

        public async Task MarkAsProcessedAsync(string eventId)
        {
            var update = Builders<OutboxEvent>.Update
                .Set(x => x.IsProcessed, true)
                .Set(x => x.ProcessedAt, DateTime.UtcNow);

            await _outboxEvents.UpdateOneAsync(x => x.Id == eventId, update);
        }

        public async Task MarkAsFailedAsync(string eventId, string errorMessage)
        {
            var update = Builders<OutboxEvent>.Update
                .Set(x => x.ErrorMessage, errorMessage);

            await _outboxEvents.UpdateOneAsync(x => x.Id == eventId, update);
        }

        public async Task MarkAsDeadLetterAsync(string eventId, string reason)
        {
            var update = Builders<OutboxEvent>.Update
                .Set(x => x.IsDeadLetter, true)
                .Set(x => x.DeadLetterReason, reason)
                .Set(x => x.DeadLetterAt, DateTime.UtcNow);

            await _outboxEvents.UpdateOneAsync(x => x.Id == eventId, update);
        }

        public async Task<IEnumerable<OutboxEvent>> GetDeadLetterEventsAsync()
        {
            return await _outboxEvents
                .Find(x => x.IsDeadLetter)
                .Sort(Builders<OutboxEvent>.Sort.Descending(x => x.DeadLetterAt))
                .ToListAsync();
        }

        public async Task ReprocessDeadLetterEventAsync(string eventId)
        {
            var update = Builders<OutboxEvent>.Update
                .Set(x => x.IsDeadLetter, false)
                .Set(x => x.IsProcessed, false)
                .Set(x => x.RetryCount, 0)
                .Set(x => x.NextRetryAt, DateTime.UtcNow) // Disponível para processamento imediato
                .Unset(x => x.DeadLetterReason)
                .Unset(x => x.DeadLetterAt);

            await _outboxEvents.UpdateOneAsync(x => x.Id == eventId, update);
        }
    }
}
