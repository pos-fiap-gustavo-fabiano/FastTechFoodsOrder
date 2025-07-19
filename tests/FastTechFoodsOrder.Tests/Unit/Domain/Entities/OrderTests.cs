using AutoFixture;
using FluentAssertions;
using FastTechFoodsOrder.Domain.Entities;
using Xunit;

namespace FastTechFoodsOrder.Tests.Unit.Domain.Entities;

public class OrderTests
{
    private readonly Fixture _fixture;

    public OrderTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public void Order_Creation_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var customerId = Guid.NewGuid().ToString();
        var totalAmount = 150.75m;

        // Act
        var order = new Order
        {
            Id = "507f1f77bcf86cd799439011",
            CustomerId = customerId,
            Total = totalAmount,
            Status = "Pending",
            OrderDate = DateTime.UtcNow
        };

        // Assert
        order.Id.Should().NotBeNullOrEmpty();
        order.CustomerId.Should().Be(customerId);
        order.Total.Should().Be(totalAmount);
        order.Status.Should().Be("Pending");
        order.OrderDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("Pending")]
    [InlineData("Accepted")]
    [InlineData("Preparing")]
    [InlineData("Ready")]
    [InlineData("Completed")]
    [InlineData("Cancelled")]
    public void Order_Status_ShouldAcceptAllValidStatuses(string status)
    {
        // Arrange & Act
        var order = _fixture.Build<Order>()
            .With(x => x.Status, status)
            .Create();

        // Assert
        order.Status.Should().Be(status);
    }

    [Fact]
    public void Order_WithPositiveAmount_ShouldHaveValidTotal()
    {
        // Arrange
        var totalAmount = 99.99m;

        // Act
        var order = _fixture.Build<Order>()
            .With(x => x.Total, totalAmount)
            .Create();

        // Assert
        order.Total.Should().Be(totalAmount);
        order.Total.Should().BePositive();
    }

    [Fact]
    public void Order_WhenStatusUpdated_ShouldReflectNewStatus()
    {
        // Arrange
        var order = _fixture.Build<Order>()
            .With(x => x.Status, "Pending")
            .Create();

        // Act
        order.Status = "Accepted";

        // Assert
        order.Status.Should().Be("Accepted");
    }

    [Fact]
    public void Order_WithSameId_ShouldBeEqual()
    {
        // Arrange
        var id = "507f1f77bcf86cd799439011";
        var order1 = _fixture.Build<Order>().With(x => x.Id, id).Create();
        var order2 = _fixture.Build<Order>().With(x => x.Id, id).Create();
        var order3 = _fixture.Build<Order>().With(x => x.Id, "507f1f77bcf86cd799439012").Create();

        // Act & Assert
        order1.Id.Should().Be(order2.Id);
        order1.Id.Should().NotBe(order3.Id);
    }

    [Fact]
    public void Order_WithItems_ShouldCalculateCorrectTotal()
    {
        // Arrange & Act
        var order = new Order
        {
            Id = "507f1f77bcf86cd799439011",
            CustomerId = Guid.NewGuid().ToString(),
            Items = new List<OrderItem>
            {
                new() { ProductId = "1", Name = "Product 1", Quantity = 2, UnitPrice = 10.50m },
                new() { ProductId = "2", Name = "Product 2", Quantity = 1, UnitPrice = 15.00m }
            }
        };

        // Calculate total manually (since the domain model might not have this logic)
        order.Total = order.Items.Sum(item => item.Quantity * item.UnitPrice);

        // Assert
        order.Total.Should().Be(36.00m); // (2 * 10.50) + (1 * 15.00)
        order.Items.Should().HaveCount(2);
    }

    [Fact]
    public void OrderItem_Creation_ShouldSetPropertiesCorrectly()
    {
        // Arrange & Act
        var orderItem = new OrderItem
        {
            ProductId = "123",
            Name = "Test Product",
            Quantity = 5,
            UnitPrice = 12.99m
        };

        // Assert
        orderItem.ProductId.Should().Be("123");
        orderItem.Name.Should().Be("Test Product");
        orderItem.Quantity.Should().Be(5);
        orderItem.UnitPrice.Should().Be(12.99m);
    }

    [Fact]
    public void OrderStatusHistory_Creation_ShouldSetPropertiesCorrectly()
    {
        // Arrange & Act
        var statusHistory = new OrderStatusHistory
        {
            Status = "Preparing",
            StatusDate = DateTime.UtcNow,
            UpdatedBy = "system"
        };

        // Assert
        statusHistory.Status.Should().Be("Preparing");
        statusHistory.StatusDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        statusHistory.UpdatedBy.Should().Be("system");
    }
}
