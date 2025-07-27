using FastTechFoodsOrder.Application.DTOs;
using FastTechFoodsOrder.Shared.Controllers;
using FastTechFoodsOrder.Shared.Results;
using FastTechFoodsOrder.Shared.Utils;
using FastTechFoodsOrder.Shared.Enums;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FastTechFoodsOrder.Api.Examples
{
    /// <summary>
    /// Exemplo de uso dos endpoints da Order API usando FastTechFoodsOrder.Shared 2.7.0
    /// </summary>
    public static class OrderApiExamples
    {
        /// <summary>
        /// Exemplo de resposta bem-sucedida para criação de pedido
        /// </summary>
        public static readonly object CreateOrderSuccessResponse = new
        {
            id = "668a1b234cd567ef890abcde",
            customerId = "customer-123",
            orderDate = "2025-07-27T18:30:00Z",
            status = "pending",
            deliveryMethod = "delivery",
            total = 31.98m,
            items = new[]
            {
                new
                {
                    productId = "prod-001",
                    name = "Hambúrguer Clássico",
                    quantity = 2,
                    unitPrice = 15.99m
                }
            }
        };

        /// <summary>
        /// Exemplo de resposta de erro padronizada usando ErrorCodes da biblioteca Shared
        /// </summary>
        public static readonly object OrderNotFoundErrorResponse = new
        {
            message = "Pedido não encontrado",
            code = "ORDER_NOT_FOUND",
            timestamp = "2025-07-27T18:30:00.000Z"
        };

        /// <summary>
        /// Exemplo de erro de validação
        /// </summary>
        public static readonly object ValidationErrorResponse = new
        {
            message = "ID do pedido é obrigatório",
            code = "VALIDATION_ERROR",
            timestamp = "2025-07-27T18:30:00.000Z"
        };

        /// <summary>
        /// Exemplo de transição de status usando OrderStatusUtils
        /// </summary>
        public static void StatusTransitionExample()
        {
            // Verificar se transição é válida
            var canTransition = OrderStatusUtils.IsValidStatusTransition(
                OrderStatus.Pending, 
                OrderStatus.Accepted
            );
            Console.WriteLine($"Pode transicionar de Pending para Accepted: {canTransition}");

            // Obter descrição do status
            var description = OrderStatusUtils.GetStatusDescription(OrderStatus.Preparing);
            Console.WriteLine($"Descrição do status Preparing: {description}");

            // Converter string para enum
            var status = OrderStatusUtils.ConvertStringToStatus("pending");
            Console.WriteLine($"Status convertido: {status}");
        }

        /// <summary>
        /// Exemplo de uso do Result Pattern no controlador
        /// </summary>
        public class ExampleOrdersController : BaseController
        {
            [HttpGet("{id}")]
            public async Task<IActionResult> GetOrderExample(string id)
            {
                // Simular chamada para serviço que retorna Result<OrderDto>
                var result = await SimulateServiceCall(id);
                
                // BaseController converte automaticamente Result em ActionResult apropriado
                return ToActionResult(result);
            }

            [HttpPost]
            public async Task<IActionResult> CreateOrderExample([FromBody] CreateOrderDto dto)
            {
                // Simular criação de pedido
                var result = await SimulateCreateOrder(dto);
                
                // Para operações de criação (201 Created)
                return ToCreatedResult(result, "GetOrderExample", new { id = result.Value?.Id });
            }

            [HttpPatch("{id}/status")]
            public async Task<IActionResult> UpdateStatusExample(string id, [FromBody] UpdateOrderStatusDto dto)
            {
                // Simular atualização de status
                var result = await SimulateUpdateStatus(id, dto);
                
                // Para operações de atualização (204 No Content)
                return ToNoContentResult(result);
            }

            private async Task<Result<OrderDto>> SimulateServiceCall(string id)
            {
                await Task.Delay(1); // Simular operação assíncrona
                
                if (string.IsNullOrEmpty(id))
                    return Result<OrderDto>.Failure("ID é obrigatório", ErrorCodes.ValidationError);
                
                if (id == "not-found")
                    return Result<OrderDto>.Failure("Pedido não encontrado", ErrorCodes.OrderNotFound);
                
                return Result<OrderDto>.Success(new OrderDto
                {
                    Id = id,
                    CustomerId = "customer-123",
                    Status = "pending",
                    DeliveryMethod = "delivery",
                    Total = 31.98m,
                    OrderDate = DateTime.UtcNow,
                    Items = new List<OrderItemDto>(),
                    StatusHistory = new List<OrderStatusHistoryDto>()
                });
            }

            private async Task<Result<OrderDto>> SimulateCreateOrder(CreateOrderDto dto)
            {
                await Task.Delay(1);
                
                return Result<OrderDto>.Success(new OrderDto
                {
                    Id = Guid.NewGuid().ToString(),
                    CustomerId = dto.CustomerId,
                    Status = "pending",
                    DeliveryMethod = dto.DeliveryMethod,
                    Total = dto.Items.Sum(i => i.UnitPrice * i.Quantity),
                    OrderDate = DateTime.UtcNow,
                    Items = dto.Items.Select(i => new OrderItemDto
                    {
                        ProductId = i.ProductId,
                        Name = i.Name,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice
                    }).ToList(),
                    StatusHistory = new List<OrderStatusHistoryDto>()
                });
            }

            private async Task<Result> SimulateUpdateStatus(string id, UpdateOrderStatusDto dto)
            {
                await Task.Delay(1);
                
                if (id == "not-found")
                    return Result.Failure("Pedido não encontrado", ErrorCodes.OrderNotFound);
                
                return Result.Success();
            }
        }
    }

    /// <summary>
    /// Códigos de erro padronizados da biblioteca Shared 2.7.0
    /// </summary>
    public static class ErrorCodes
    {
        // Gerais
        public const string ValidationError = "VALIDATION_ERROR";
        public const string NotFound = "NOT_FOUND";
        public const string InternalError = "INTERNAL_ERROR";
        
        // Pedidos
        public const string OrderNotFound = "ORDER_NOT_FOUND";
        public const string OrderInvalidStatus = "ORDER_INVALID_STATUS";
        public const string OrderStatusTransitionInvalid = "ORDER_STATUS_TRANSITION_INVALID";
        
        // Produtos
        public const string ProductNotFound = "PRODUCT_NOT_FOUND";
        public const string ProductOutOfStock = "PRODUCT_OUT_OF_STOCK";
        
        // Pagamentos
        public const string PaymentFailed = "PAYMENT_FAILED";
        public const string PaymentMethodInvalid = "PAYMENT_METHOD_INVALID";
    }
}
