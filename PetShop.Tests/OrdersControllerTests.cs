using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using PetShop.Api;
using PetShop.Api.DTOs;
using PetShop.Application.DTOs;
using PetShop.Domain;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace PetShop.Tests;

/// <summary>
/// Integration tests for OrdersController.
/// Tests verify all order endpoints work correctly with real application services.
/// </summary>
public class OrdersControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public OrdersControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    #region Get All Orders Tests

    [Fact]
    public async Task GetAllOrders_ReturnsListOfOrders()
    {
        // Arrange - Create some orders
        var order1 = await CreateTestOrderAsync();
        var order2 = await CreateTestOrderAsync();

        // Act
        var response = await _client.GetAsync("/api/v1/orders");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var orders = await response.Content.ReadFromJsonAsync<OrderDto[]>(_jsonOptions);
        orders.Should().NotBeNull();
        orders!.Length.Should().BeGreaterThanOrEqualTo(2);
        orders.Should().Contain(o => o.Id == order1.Id);
        orders.Should().Contain(o => o.Id == order2.Id);
    }

    #endregion

    #region Create Order Tests

    [Fact]
    public async Task CreateOrder_ValidRequest_Returns201Created()
    {
        // Arrange - Create a customer first
        var customer = await CreateTestCustomerAsync();
        var request = new CreateOrderRequest
        {
            CustomerId = customer.Id,
            PickupDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1))
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var order = await response.Content.ReadFromJsonAsync<OrderDto>(_jsonOptions);
        order.Should().NotBeNull();
        order!.CustomerId.Should().Be(customer.Id);
        order.PickupDate.Should().Be(request.PickupDate);
        order.Status.Should().Be(OrderStatus.Open);
        order.Pets.Should().BeEmpty();
        order.EstimatedCost.Should().Be(0m);
        order.ActualCost.Should().BeNull();
    }

    [Fact]
    public async Task CreateOrder_NonExistentCustomer_Returns404NotFound()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            CustomerId = Guid.NewGuid(),
            PickupDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1))
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateOrder_NullRequest_Returns400BadRequest()
    {
        // Act
        var response = await _client.PostAsJsonAsync<CreateOrderRequest>("/api/v1/orders", null!);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateOrder_PickupDateInPast_Returns422UnprocessableEntity()
    {
        // Arrange - Task: "Pickup Date must be today or in the future"
        var customer = await CreateTestCustomerAsync();
        var request = new CreateOrderRequest
        {
            CustomerId = customer.Id,
            PickupDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1))
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    #endregion

    #region Get Order Tests

    [Fact]
    public async Task GetOrder_ExistingOrder_Returns200OkWithCostCalculations()
    {
        // Arrange - Create an order
        var order = await CreateTestOrderAsync();

        // Act
        var response = await _client.GetAsync($"/api/v1/orders/{order.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var retrievedOrder = await response.Content.ReadFromJsonAsync<OrderDto>(_jsonOptions);
        retrievedOrder.Should().NotBeNull();
        retrievedOrder!.Id.Should().Be(order.Id);
        retrievedOrder.Status.Should().Be(OrderStatus.Open);
        retrievedOrder.EstimatedCost.Should().Be(0m);
        retrievedOrder.ActualCost.Should().BeNull();
    }

    [Fact]
    public async Task GetOrder_NonExistentOrder_Returns404NotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/v1/orders/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Update Order Tests

    [Fact]
    public async Task UpdateOrder_ExistingOrder_Returns200Ok()
    {
        // Arrange - Create an order
        var order = await CreateTestOrderAsync();
        var newPickupDate = DateOnly.FromDateTime(DateTime.Today.AddDays(2));
        var updateRequest = new UpdateOrderRequest
        {
            PickupDate = newPickupDate
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/v1/orders/{order.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedOrder = await response.Content.ReadFromJsonAsync<OrderDto>(_jsonOptions);
        updatedOrder.Should().NotBeNull();
        updatedOrder!.PickupDate.Should().Be(newPickupDate);
    }

    [Fact]
    public async Task UpdateOrder_ProcessingOrder_CanUpdatePickupDateOnly_Returns200Ok()
    {
        // Task: "When an order is 'Processing' only the Pickup Date can be edited."
        var order = await CreateTestOrderAsync();
        await AddPetToOrderAsync(order.Id);
        await TransitionOrderAsync(order.Id, OrderStatus.Processing);

        var newPickupDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5));
        var updateRequest = new UpdateOrderRequest { PickupDate = newPickupDate };

        var response = await _client.PatchAsJsonAsync($"/api/v1/orders/{order.Id}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<OrderDto>(_jsonOptions);
        updated!.Status.Should().Be(OrderStatus.Processing);
        updated.PickupDate.Should().Be(newPickupDate);
    }

    [Fact]
    public async Task UpdateOrder_DeliveredOrder_Returns409Conflict()
    {
        // Arrange - Create and deliver an order
        var order = await CreateTestOrderAsync();
        await AddPetToOrderAsync(order.Id);
        await TransitionOrderAsync(order.Id, OrderStatus.Processing);
        await TransitionOrderAsync(order.Id, OrderStatus.Delivered);

        var updateRequest = new UpdateOrderRequest
        {
            PickupDate = DateOnly.FromDateTime(DateTime.Today.AddDays(3))
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/v1/orders/{order.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateOrder_NonExistentOrder_Returns404NotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var updateRequest = new UpdateOrderRequest
        {
            PickupDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1))
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/v1/orders/{nonExistentId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region State Transition Tests

    [Fact]
    public async Task TransitionOrder_OpenToProcessing_Returns200Ok()
    {
        // Arrange - Create order with pet
        var order = await CreateTestOrderAsync();
        await AddPetToOrderAsync(order.Id);

        var transitionRequest = new TransitionOrderRequest
        {
            Status = OrderStatus.Processing
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/orders/{order.Id}/transition", transitionRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedOrder = await response.Content.ReadFromJsonAsync<OrderDto>(_jsonOptions);
        updatedOrder.Should().NotBeNull();
        updatedOrder!.Status.Should().Be(OrderStatus.Processing);
    }

    [Fact]
    public async Task TransitionOrder_ProcessingToDelivered_Returns200Ok()
    {
        // Arrange - Create order, add pet, transition to Processing
        var order = await CreateTestOrderAsync();
        await AddPetToOrderAsync(order.Id);
        await TransitionOrderAsync(order.Id, OrderStatus.Processing);

        var transitionRequest = new TransitionOrderRequest
        {
            Status = OrderStatus.Delivered
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/orders/{order.Id}/transition", transitionRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedOrder = await response.Content.ReadFromJsonAsync<OrderDto>(_jsonOptions);
        updatedOrder.Should().NotBeNull();
        updatedOrder!.Status.Should().Be(OrderStatus.Delivered);
        updatedOrder.ActualCost.Should().NotBeNull();
        updatedOrder.EstimatedCost.Should().BeNull();
    }

    [Fact]
    public async Task TransitionOrder_NoPets_Returns409Conflict()
    {
        // Arrange - Create order without pets
        var order = await CreateTestOrderAsync();

        var transitionRequest = new TransitionOrderRequest
        {
            Status = OrderStatus.Processing
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/orders/{order.Id}/transition", transitionRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task TransitionOrder_InvalidTransition_Returns409Conflict()
    {
        // Arrange - Create order and transition to Processing
        var order = await CreateTestOrderAsync();
        await AddPetToOrderAsync(order.Id);
        await TransitionOrderAsync(order.Id, OrderStatus.Processing);

        // Try to transition back to Open (invalid)
        var transitionRequest = new TransitionOrderRequest
        {
            Status = OrderStatus.Open
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/orders/{order.Id}/transition", transitionRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task TransitionOrder_NonExistentOrder_Returns404NotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var transitionRequest = new TransitionOrderRequest
        {
            Status = OrderStatus.Processing
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/orders/{nonExistentId}/transition", transitionRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Add Pet Tests

    [Fact]
    public async Task AddPetToOrder_OpenOrder_Returns201Created()
    {
        // Arrange - Create an order
        var order = await CreateTestOrderAsync();
        var petRequest = new AddPetRequest
        {
            Name = "Fluffy",
            Price = 100.50m,
            Kind = "Cat",
            Color = "White"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/orders/{order.Id}/pets", petRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var updatedOrder = await response.Content.ReadFromJsonAsync<OrderDto>(_jsonOptions);
        updatedOrder.Should().NotBeNull();
        updatedOrder!.Pets.Should().HaveCount(1);
        updatedOrder.Pets.First().Name.Should().Be("Fluffy");
        updatedOrder.Pets.First().Price.Should().Be(100.50m);
        updatedOrder.EstimatedCost.Should().Be(100.50m);
    }

    [Fact]
    public async Task AddPetToOrder_ProcessingOrder_Returns409Conflict()
    {
        // Arrange - Create order, add pet, transition to Processing
        var order = await CreateTestOrderAsync();
        await AddPetToOrderAsync(order.Id);
        await TransitionOrderAsync(order.Id, OrderStatus.Processing);

        var petRequest = new AddPetRequest
        {
            Name = "Buddy",
            Price = 200m
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/orders/{order.Id}/pets", petRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task AddPetToOrder_NonExistentOrder_Returns404NotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var petRequest = new AddPetRequest
        {
            Name = "Test",
            Price = 50m
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/orders/{nonExistentId}/pets", petRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Remove Pet Tests

    [Fact]
    public async Task RemovePetFromOrder_OpenOrder_Returns204NoContent()
    {
        // Arrange - Create order and add pet
        var order = await CreateTestOrderAsync();
        var pet = await AddPetToOrderAsync(order.Id);

        // Act
        var response = await _client.DeleteAsync($"/api/v1/orders/{order.Id}/pets/{pet.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify pet was removed
        var getResponse = await _client.GetAsync($"/api/v1/orders/{order.Id}");
        var updatedOrder = await getResponse.Content.ReadFromJsonAsync<OrderDto>(_jsonOptions);
        updatedOrder!.Pets.Should().BeEmpty();
        updatedOrder.EstimatedCost.Should().Be(0m);
    }

    [Fact]
    public async Task RemovePetFromOrder_ProcessingOrder_Returns409Conflict()
    {
        // Arrange - Create order, add pet, transition to Processing
        var order = await CreateTestOrderAsync();
        var pet = await AddPetToOrderAsync(order.Id);
        await TransitionOrderAsync(order.Id, OrderStatus.Processing);

        // Act
        var response = await _client.DeleteAsync($"/api/v1/orders/{order.Id}/pets/{pet.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task RemovePetFromOrder_NonExistentOrder_Returns404NotFound()
    {
        // Arrange
        var nonExistentOrderId = Guid.NewGuid();
        var petId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/v1/orders/{nonExistentOrderId}/pets/{petId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemovePetFromOrder_NonExistentPet_Returns404NotFound()
    {
        // Arrange - Create order
        var order = await CreateTestOrderAsync();
        var nonExistentPetId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/v1/orders/{order.Id}/pets/{nonExistentPetId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Cost Calculation Tests

    [Fact]
    public async Task GetOrder_OpenOrder_ReturnsEstimatedCost()
    {
        // Arrange - Create order with pets
        var order = await CreateTestOrderAsync();
        await AddPetToOrderAsync(order.Id, "Pet1", 100m);
        await AddPetToOrderAsync(order.Id, "Pet2", 200m);

        // Act
        var response = await _client.GetAsync($"/api/v1/orders/{order.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var retrievedOrder = await response.Content.ReadFromJsonAsync<OrderDto>(_jsonOptions);
        retrievedOrder!.EstimatedCost.Should().Be(300m);
        retrievedOrder.ActualCost.Should().BeNull();
    }

    [Fact]
    public async Task GetOrder_DeliveredOrder_ReturnsActualCost()
    {
        // Arrange - Create order, add pets, transition to Delivered
        var order = await CreateTestOrderAsync();
        await AddPetToOrderAsync(order.Id, "Pet1", 150m);
        await TransitionOrderAsync(order.Id, OrderStatus.Processing);
        await TransitionOrderAsync(order.Id, OrderStatus.Delivered);

        // Act
        var response = await _client.GetAsync($"/api/v1/orders/{order.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var retrievedOrder = await response.Content.ReadFromJsonAsync<OrderDto>(_jsonOptions);
        retrievedOrder!.ActualCost.Should().Be(150m);
        retrievedOrder.EstimatedCost.Should().BeNull();
    }

    #endregion

    #region Helper Methods

    private async Task<CustomerDto> CreateTestCustomerAsync()
    {
        var request = new CreateCustomerRequest
        {
            FirstName = "Test",
            LastName = "Customer",
            Email = "test@example.com"
        };
        var response = await _client.PostAsJsonAsync("/api/v1/customers", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CustomerDto>(_jsonOptions) ?? throw new Exception("Failed to create customer");
    }

    private async Task<OrderDto> CreateTestOrderAsync()
    {
        var customer = await CreateTestCustomerAsync();
        var request = new CreateOrderRequest
        {
            CustomerId = customer.Id,
            PickupDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1))
        };
        var response = await _client.PostAsJsonAsync("/api/v1/orders", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<OrderDto>(_jsonOptions) ?? throw new Exception("Failed to create order");
    }

    private async Task<PetDto> AddPetToOrderAsync(Guid orderId, string name = "Test Pet", decimal price = 100m)
    {
        var request = new AddPetRequest
        {
            Name = name,
            Price = price
        };
        var response = await _client.PostAsJsonAsync($"/api/v1/orders/{orderId}/pets", request);

        // Only ensure success if we got a successful status code
        // Some tests intentionally test error scenarios (like 409 Conflict)
        if (response.IsSuccessStatusCode)
        {
            var order = await response.Content.ReadFromJsonAsync<OrderDto>(_jsonOptions) ?? throw new Exception("Failed to add pet");
            return order.Pets.Last();
        }

        // If not successful, throw with the actual status code for debugging
        throw new HttpRequestException($"Request failed with status {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
    }

    private async Task TransitionOrderAsync(Guid orderId, OrderStatus targetStatus)
    {
        var request = new TransitionOrderRequest
        {
            Status = targetStatus
        };
        var response = await _client.PostAsJsonAsync($"/api/v1/orders/{orderId}/transition", request);
        response.EnsureSuccessStatusCode();
    }

    #endregion
}
