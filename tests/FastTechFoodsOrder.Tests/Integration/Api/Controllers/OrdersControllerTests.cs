using System.Net;
using System.Text;
using System.Text.Json;
using AutoFixture;
using FluentAssertions;
using FastTechFoodsOrder.Application.DTOs;
using FastTechFoodsOrder.Tests.Integration.Setup;
using Xunit;

namespace FastTechFoodsOrder.Tests.Integration.Api.Controllers;

public class OrdersControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;
    private readonly Fixture _fixture;
    private readonly JsonSerializerOptions _jsonOptions;

    public OrdersControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _fixture = new Fixture();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    [Fact]
    public async Task CreateOrder_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var createOrderDto = new CreateOrderDto
        {
            CustomerId = Guid.NewGuid().ToString(),
            DeliveryMethod = "delivery",
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = "1", Name = "Test Product", Quantity = 2, UnitPrice = 25.50m }
            }
        };

        var json = JsonSerializer.Serialize(createOrderDto, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/orders", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var createdOrder = JsonSerializer.Deserialize<OrderDto>(responseContent, _jsonOptions);
        
        createdOrder.Should().NotBeNull();
        createdOrder.CustomerId.Should().Be(createOrderDto.CustomerId);
        createdOrder.Status.Should().Be("Pending");
    }

    [Fact]
    public async Task GetOrder_WithExistingId_ShouldReturnOrder()
    {
        // Arrange
        var createOrderDto = new CreateOrderDto
        {
            CustomerId = Guid.NewGuid().ToString(),
            DeliveryMethod = "pickup",
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = "2", Name = "Another Product", Quantity = 1, UnitPrice = 15.99m }
            }
        };

        var createJson = JsonSerializer.Serialize(createOrderDto, _jsonOptions);
        var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
        
        var createResponse = await _client.PostAsync("/api/orders", createContent);
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createdOrder = JsonSerializer.Deserialize<OrderDto>(createResponseContent, _jsonOptions);

        // Act
        var response = await _client.GetAsync($"/api/orders/{createdOrder.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var retrievedOrder = JsonSerializer.Deserialize<OrderDto>(responseContent, _jsonOptions);
        
        retrievedOrder.Should().NotBeNull();
        retrievedOrder.Id.Should().Be(createdOrder.Id);
        retrievedOrder.CustomerId.Should().Be(createOrderDto.CustomerId);
    }

    [Fact]
    public async Task GetOrder_WithNonExistingId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistingId = "507f1f77bcf86cd799439011";

        // Act
        var response = await _client.GetAsync($"/api/orders/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllOrders_ShouldReturnOrdersList()
    {
        // Arrange
        var createOrderDto = new CreateOrderDto
        {
            CustomerId = Guid.NewGuid().ToString(),
            DeliveryMethod = "delivery",
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = "3", Name = "Third Product", Quantity = 3, UnitPrice = 10.00m }
            }
        };

        var json = JsonSerializer.Serialize(createOrderDto, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        await _client.PostAsync("/api/orders", content);

        // Act
        var response = await _client.GetAsync("/api/orders");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var orders = JsonSerializer.Deserialize<List<OrderDto>>(responseContent, _jsonOptions);
        
        orders.Should().NotBeNull();
        orders.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UpdateOrderStatus_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var createOrderDto = new CreateOrderDto
        {
            CustomerId = Guid.NewGuid().ToString(),
            DeliveryMethod = "delivery",
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = "4", Name = "Fourth Product", Quantity = 1, UnitPrice = 50.00m }
            }
        };

        var createJson = JsonSerializer.Serialize(createOrderDto, _jsonOptions);
        var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
        
        var createResponse = await _client.PostAsync("/api/orders", createContent);
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createdOrder = JsonSerializer.Deserialize<OrderDto>(createResponseContent, _jsonOptions);

        var updateStatusDto = new UpdateOrderStatusDto
        {
            Status = "Preparing",
            UpdatedBy = "system"
        };

        var updateJson = JsonSerializer.Serialize(updateStatusDto, _jsonOptions);
        var updateContent = new StringContent(updateJson, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/api/orders/{createdOrder.Id}/status", updateContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateOrderStatus_WithNonExistingOrder_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistingId = "507f1f77bcf86cd799439012";
        var updateStatusDto = new UpdateOrderStatusDto
        {
            Status = "Preparing",
            UpdatedBy = "system"
        };

        var json = JsonSerializer.Serialize(updateStatusDto, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/api/orders/{nonExistingId}/status", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateOrder_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidOrderDto = new CreateOrderDto
        {
            CustomerId = string.Empty, // Invalid: empty customer ID
            DeliveryMethod = "delivery",
            Items = new List<CreateOrderItemDto>()
        };

        var json = JsonSerializer.Serialize(invalidOrderDto, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/orders", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
