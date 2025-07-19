using FastTechFoodsOrder.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FastTechFoodsOrder.Api.Controllers
{
    [ApiController]
    [Route("api/outbox")]
    public class OutboxController : ControllerBase
    {
        private readonly IOutboxRepository _outboxRepository;

        public OutboxController(IOutboxRepository outboxRepository)
        {
            _outboxRepository = outboxRepository;
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingEvents()
        {
            var events = await _outboxRepository.GetUnprocessedEventsAsync();
            return Ok(new
            {
                Count = events.Count(),
                Events = events.Select(e => new
                {
                    e.Id,
                    e.EventType,
                    e.CreatedAt,
                    e.RetryCount,
                    e.AggregateId
                })
            });
        }

        [HttpGet("dead-letter")]
        public async Task<IActionResult> GetDeadLetterEvents()
        {
            var events = await _outboxRepository.GetDeadLetterEventsAsync();
            return Ok(new
            {
                Count = events.Count(),
                Events = events.Select(e => new
                {
                    e.Id,
                    e.EventType,
                    e.CreatedAt,
                    e.RetryCount,
                    e.DeadLetterReason,
                    e.DeadLetterAt
                })
            });
        }

        [HttpPost("reprocess/{eventId}")]
        public async Task<IActionResult> ReprocessEvent(string eventId)
        {
            await _outboxRepository.ReprocessDeadLetterEventAsync(eventId);
            return Ok(new { Message = "Event queued for reprocessing", EventId = eventId });
        }
    }
}
