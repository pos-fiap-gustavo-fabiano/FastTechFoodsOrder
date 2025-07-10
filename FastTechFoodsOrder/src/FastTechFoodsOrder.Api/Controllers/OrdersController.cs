using FastTechFoodsOrder.Application.DTOs;
using FastTechFoodsOrder.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace FastTechFoodsOrder.Api.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // GET /api/orders?customerId=xxx
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders([FromQuery] string? customerId)
        {
            var orders = await _orderService.GetOrdersAsync(customerId);
            return Ok(orders);
        }

        // GET /api/orders/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrderById(string? id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound();
            return Ok(order);
        }

        // POST /api/orders
        [HttpPost]
        public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderDto dto,
            [FromServices] IValidator<CreateOrderDto> validator)
        {
            var validationResult = await validator.ValidateAsync(dto);
            if (validationResult.IsValid) {
                return BadRequest(validationResult.Errors);
            }
            var created = await _orderService.CreateOrderAsync(dto);
            return CreatedAtAction(nameof(GetOrderById), new { id = created.Id }, created);
        }

        // PATCH /api/orders/{id}/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(string id, [FromBody] UpdateOrderStatusDto dto)
        {
            var updated = await _orderService.UpdateOrderStatusAsync(id, dto);
            if (!updated)
                return NotFound();
            return NoContent();
        }
    }
}
