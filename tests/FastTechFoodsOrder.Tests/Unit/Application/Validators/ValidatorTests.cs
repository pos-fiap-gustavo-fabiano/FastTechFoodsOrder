using AutoFixture;
using FluentAssertions;
using FluentValidation;
using FluentValidation.TestHelper;
using FastTechFoodsOrder.Application.DTOs;
using FastTechFoodsOrder.Application.Validators;
using Xunit;

namespace FastTechFoodsOrder.Tests.Unit.Application.Validators;

public class ValidatorTests
{
    private readonly Fixture _fixture;
    private readonly CreateOrderRequestValidator _createOrderValidator;
    private readonly UpdateOrderSatusValidator _updateOrderStatusValidator;

    public ValidatorTests()
    {
        _fixture = new Fixture();
        _createOrderValidator = new CreateOrderRequestValidator();
        _updateOrderStatusValidator = new UpdateOrderSatusValidator();
    }

    #region CreateOrderRequestValidator Tests

    [Fact]
    public void CreateOrderValidator_WithValidRequest_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var request = new CreateOrderDto
        {
            CustomerId = Guid.NewGuid().ToString(),
            DeliveryMethod = "delivery",
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = "1", Name = "Product 1", Quantity = 2, UnitPrice = 10.99m }
            }
        };

        // Act
        var result = _createOrderValidator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateOrderValidator_WithEmptyCustomerId_ShouldHaveValidationError()
    {
        // Arrange
        var request = new CreateOrderDto
        {
            CustomerId = string.Empty,
            DeliveryMethod = "delivery",
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = "1", Name = "Product 1", Quantity = 1, UnitPrice = 10.99m }
            }
        };

        // Act
        var result = _createOrderValidator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CustomerId)
              .WithErrorMessage("ID do cliente é obrigatório");
    }

    [Fact]
    public void CreateOrderValidator_WithEmptyDeliveryMethod_ShouldHaveValidationError()
    {
        // Arrange
        var request = new CreateOrderDto
        {
            CustomerId = Guid.NewGuid().ToString(),
            DeliveryMethod = string.Empty,
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = "1", Name = "Product 1", Quantity = 1, UnitPrice = 10.99m }
            }
        };

        // Act
        var result = _createOrderValidator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DeliveryMethod)
              .WithErrorMessage("O método de entrega é obrigatório.");
    }

    [Fact]
    public void CreateOrderValidator_WithEmptyItems_ShouldHaveValidationError()
    {
        // Arrange
        var request = new CreateOrderDto
        {
            CustomerId = Guid.NewGuid().ToString(),
            DeliveryMethod = "delivery",
            Items = new List<CreateOrderItemDto>()
        };

        // Act
        var result = _createOrderValidator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Items)
              .WithErrorMessage("Pelo menos um item do pedido é obrigatório.");
    }

    [Fact]
    public void CreateOrderValidator_WithZeroQuantityItems_ShouldHaveValidationError()
    {
        // Arrange
        var request = new CreateOrderDto
        {
            CustomerId = Guid.NewGuid().ToString(),
            DeliveryMethod = "delivery",
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = "1", Name = "Product 1", Quantity = 0, UnitPrice = 10.99m }
            }
        };

        // Act
        var result = _createOrderValidator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Items)
              .WithErrorMessage("Todos os itens do pedido devem ter uma quantidade maior que zero.");
    }

    [Fact]
    public void CreateOrderValidator_WithNegativeQuantityItems_ShouldHaveValidationError()
    {
        // Arrange
        var request = new CreateOrderDto
        {
            CustomerId = Guid.NewGuid().ToString(),
            DeliveryMethod = "delivery",
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = "1", Name = "Product 1", Quantity = -1, UnitPrice = 10.99m }
            }
        };

        // Act
        var result = _createOrderValidator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Items)
              .WithErrorMessage("Todos os itens do pedido devem ter uma quantidade maior que zero.");
    }

    #endregion

    #region UpdateOrderStatusValidator Tests

    [Theory]
    [InlineData("Pending")]
    [InlineData("Accepted")]
    [InlineData("Preparing")]
    [InlineData("Ready")]
    [InlineData("Completed")]
    [InlineData("Cancelled")]
    public void UpdateOrderStatusValidator_WithValidStatus_ShouldNotHaveValidationError(string status)
    {
        // Arrange
        var request = new UpdateOrderStatusDto
        {
            Status = status,
            UpdatedBy = "system"
        };

        // Act
        var result = _updateOrderStatusValidator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void UpdateOrderStatusValidator_WithEmptyStatus_ShouldHaveValidationError()
    {
        // Arrange
        var request = new UpdateOrderStatusDto
        {
            Status = string.Empty,
            UpdatedBy = "system"
        };

        // Act
        var result = _updateOrderStatusValidator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Status)
              .WithErrorMessage("O status do pedido é obrigatório.");
    }

    [Fact]
    public void UpdateOrderStatusValidator_WithNullStatus_ShouldHaveValidationError()
    {
        // Arrange
        var request = new UpdateOrderStatusDto
        {
            Status = null!,
            UpdatedBy = "system"
        };

        // Act
        var result = _updateOrderStatusValidator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Status)
              .WithErrorMessage("O status do pedido é obrigatório.");
    }

    #endregion
}
