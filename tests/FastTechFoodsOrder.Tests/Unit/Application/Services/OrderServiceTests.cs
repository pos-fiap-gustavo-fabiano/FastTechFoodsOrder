using AutoFixture;
using FluentAssertions;
using Moq;
using Xunit;
using FastTechFoodsOrder.Application.Services;
using FastTechFoodsOrder.Application.Interfaces;
using FastTechFoodsOrder.Application.DTOs;
using FastTechFoodsOrder.Domain.Entities;

namespace FastTechFoodsOrder.Tests.Unit.Application.Services;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _mockOrderRepository;
    private readonly Mock<IOutboxRepository> _mockOutboxRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly OrderService _orderService;
    private readonly Fixture _fixture;

    public OrderServiceTests()
    {
        _mockOrderRepository = new Mock<IOrderRepository>();
        _mockOutboxRepository = new Mock<IOutboxRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _orderService = new OrderService(_mockOrderRepository.Object, _mockOutboxRepository.Object, _mockUnitOfWork.Object);
        _fixture = new Fixture();
    }

    [Fact]
    public async Task GetOrderByIdAsync_WithExistingId_ShouldReturnOrderDto()
    {
        // Arrange
        var orderId = "order-123";
        var order = new Order
        {
            Id = orderId,
            CustomerId = "customer-123",
            DeliveryMethod = "delivery",
            Status = "Pending",
            OrderDate = DateTime.UtcNow,
            Total = 15.00m,
            Items = new List<OrderItem>
            {
                new() { ProductId = "1", Name = "Product 1", Quantity = 1, UnitPrice = 15.00m }
            },
            StatusHistory = new List<OrderStatusHistory>()
        };

        _mockOrderRepository.Setup(x => x.GetOrderByIdAsync(orderId))
            .ReturnsAsync(order);

        // Act
        var result = await _orderService.GetOrderByIdAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(order.Id);
        result.Value.CustomerId.Should().Be(order.CustomerId);
    }

    [Fact]
    public async Task GetOrderByIdAsync_WithNonExistingId_ShouldReturnFailure()
    {
        // Arrange
        var orderId = "non-existing-id";
        _mockOrderRepository.Setup(x => x.GetOrderByIdAsync(orderId))
            .ReturnsAsync((Order?)null);

        // Act
        var result = await _orderService.GetOrderByIdAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("não encontrado");
    }

    [Fact]
    public async Task GetOrdersAsync_ShouldReturnOrderDtoList()
    {
        // Arrange
        var orders = new List<Order>
        {
            new() 
            { 
                Id = "1", 
                CustomerId = "customer-1", 
                DeliveryMethod = "delivery", 
                Status = "Pending",
                OrderDate = DateTime.UtcNow,
                Total = 10.00m,
                Items = new List<OrderItem>(),
                StatusHistory = new List<OrderStatusHistory>()
            },
            new() 
            { 
                Id = "2", 
                CustomerId = "customer-2", 
                DeliveryMethod = "pickup", 
                Status = "Pending",
                OrderDate = DateTime.UtcNow,
                Total = 15.00m,
                Items = new List<OrderItem>(),
                StatusHistory = new List<OrderStatusHistory>()
            }
        };

        _mockOrderRepository.Setup(x => x.GetOrdersAsync(null))
            .ReturnsAsync(orders);

        // Act
        var result = await _orderService.GetOrdersAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_WithNonExistingOrder_ShouldReturnFalse()
    {
        // Arrange
        var orderId = "non-existing-id";
        var updateDto = new UpdateOrderStatusDto
        {
            Status = "Preparing",
            UpdatedBy = "system"
        };

        _mockOrderRepository.Setup(x => x.GetOrderByIdAsync(orderId))
            .ReturnsAsync((Order?)null);

        // Act
        var result = await _orderService.UpdateOrderStatusAsync(orderId, updateDto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("não encontrado");
    }
}
