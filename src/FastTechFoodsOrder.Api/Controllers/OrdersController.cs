using FastTechFoodsOrder.Application.DTOs;
using FastTechFoodsOrder.Application.Interfaces;
using FastTechFoodsOrder.Shared.Controllers;
using FastTechFoodsOrder.Shared.Results;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FastTechFoodsOrder.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/orders")]
    public class OrdersController : BaseController
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // GET /api/orders?customerId=xxx
        [HttpGet]
        public async Task<IActionResult> GetOrders([FromQuery] string? customerId)
        {
            var result = await _orderService.GetOrdersAsync(customerId);
            return ToActionResult(result);
        }

        // GET /api/orders/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(string? id)
        {
            var result = await _orderService.GetOrderByIdAsync(id);
            return ToActionResult(result);
        }

        // POST /api/orders
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto,
            [FromServices] IValidator<CreateOrderDto> validator)
        {
            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid) {
                return BadRequest(validationResult.Errors);
            }
            var result = await _orderService.CreateOrderAsync(dto);
            return Ok(result.Value);
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(string id, [FromBody] UpdateOrderStatusDto dto)
        {
            var result = await _orderService.UpdateOrderStatusAsync(id, dto);
            return ToNoContentResult(result);
        }

        [HttpPut("cancel/{id}")]
        public async Task<IActionResult> OrderCancel(string id, [FromBody] UpdateOrderStatusDto dto)
        {
            var result = await _orderService.CancelOrderAsync(id, dto);
            return ToNoContentResult(result);
        }
    }
}
