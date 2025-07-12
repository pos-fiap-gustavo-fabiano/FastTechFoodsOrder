using FastTechFoodsOrder.Domain.Entities;
using MongoDB.Driver;

namespace FastTechFoodsOrder.Application.Interfaces
{
    public interface IOutboxRepository
    {
        Task AddEventAsync(OutboxEvent outboxEvent);
        Task<IEnumerable<OutboxEvent>> GetUnprocessedEventsAsync(int batchSize = 50);
        Task MarkAsProcessedAsync(string eventId);
        Task MarkAsFailedAsync(string eventId, string errorMessage);
        Task IncrementRetryCountAsync(string eventId);
        
        // Novo método com suporte a sessão transacional
        Task AddEventAsync(OutboxEvent outboxEvent, IClientSessionHandle session);

        Task MarkAsDeadLetterAsync(string eventId, string reason);
        Task<IEnumerable<OutboxEvent>> GetDeadLetterEventsAsync();
        Task ReprocessDeadLetterEventAsync(string eventId);
    }
}
