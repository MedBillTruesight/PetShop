using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using PetShop.Api;
using PetShop.Application.DTOs;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace PetShop.Tests;

/// <summary>
/// Integration tests for CustomersController.
/// Tests verify all customer endpoints work correctly with real application services.
/// </summary>
public class CustomersControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public CustomersControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    [Fact]
    public async Task CreateCustomer_ValidRequest_Returns201Created()
    {
        // Arrange
        var request = new CreateCustomerRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Phone = "555-1234"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/customers", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var customer = await response.Content.ReadFromJsonAsync<CustomerDto>(_jsonOptions);
        customer.Should().NotBeNull();
        customer!.FirstName.Should().Be("John");
        customer.LastName.Should().Be("Doe");
        customer.Email.Should().Be("john.doe@example.com");
        customer.Phone.Should().Be("555-1234");
        customer.Id.Should().NotBeEmpty();
        customer.EstimatedPaymentDue.Should().Be(0m);
        customer.ActualPaymentDue.Should().Be(0m);
        
        // Verify Location header (accepts both v1 and v1.0 format)
        response.Headers.Location.Should().NotBeNull();
        var location = response.Headers.Location!.ToString();
        location.Should().Contain("/api/v1");
        location.Should().Contain($"/customers/{customer.Id}");
    }

    [Fact]
    public async Task CreateCustomer_MinimalRequest_Returns201Created()
    {
        // Arrange
        var request = new CreateCustomerRequest
        {
            FirstName = "Jane",
            LastName = "Smith"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/customers", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var customer = await response.Content.ReadFromJsonAsync<CustomerDto>(_jsonOptions);
        customer.Should().NotBeNull();
        customer!.FirstName.Should().Be("Jane");
        customer.LastName.Should().Be("Smith");
        customer.Email.Should().BeNull();
        customer.Phone.Should().BeNull();
    }

    [Fact]
    public async Task CreateCustomer_NullRequest_Returns400BadRequest()
    {
        // Act
        var response = await _client.PostAsJsonAsync<CreateCustomerRequest>("/api/v1/customers", null!);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetCustomer_ExistingCustomer_Returns200OkWithPaymentCalculations()
    {
        // Arrange - Create a customer first
        var createRequest = new CreateCustomerRequest
        {
            FirstName = "Test",
            LastName = "Customer",
            Email = "test@example.com"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/customers", createRequest);
        var createdCustomer = await createResponse.Content.ReadFromJsonAsync<CustomerDto>(_jsonOptions);

        // Act
        var response = await _client.GetAsync($"/api/v1/customers/{createdCustomer!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var customer = await response.Content.ReadFromJsonAsync<CustomerDto>(_jsonOptions);
        customer.Should().NotBeNull();
        customer!.Id.Should().Be(createdCustomer.Id);
        customer.FirstName.Should().Be("Test");
        customer.LastName.Should().Be("Customer");
        customer.Email.Should().Be("test@example.com");
        customer.EstimatedPaymentDue.Should().Be(0m);
        customer.ActualPaymentDue.Should().Be(0m);
    }

    [Fact]
    public async Task GetCustomer_NonExistentCustomer_Returns404NotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/v1/customers/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateCustomer_ExistingCustomer_Returns200Ok()
    {
        // Arrange - Create a customer first
        var createRequest = new CreateCustomerRequest
        {
            FirstName = "Original",
            LastName = "Name",
            Email = "original@example.com"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/customers", createRequest);
        var createdCustomer = await createResponse.Content.ReadFromJsonAsync<CustomerDto>(_jsonOptions);

        var updateRequest = new UpdateCustomerRequest
        {
            FirstName = "Updated",
            LastName = "Name",
            Email = "updated@example.com",
            Phone = "555-9999"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/customers/{createdCustomer!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var customer = await response.Content.ReadFromJsonAsync<CustomerDto>(_jsonOptions);
        customer.Should().NotBeNull();
        customer!.Id.Should().Be(createdCustomer.Id);
        customer.FirstName.Should().Be("Updated");
        customer.LastName.Should().Be("Name");
        customer.Email.Should().Be("updated@example.com");
        customer.Phone.Should().Be("555-9999");
    }

    [Fact]
    public async Task UpdateCustomer_NonExistentCustomer_Returns404NotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var updateRequest = new UpdateCustomerRequest
        {
            FirstName = "Test",
            LastName = "Customer"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/customers/{nonExistentId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateCustomer_NullRequest_Returns400BadRequest()
    {
        // Arrange - Create a customer first
        var createRequest = new CreateCustomerRequest
        {
            FirstName = "Test",
            LastName = "Customer"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/customers", createRequest);
        var createdCustomer = await createResponse.Content.ReadFromJsonAsync<CustomerDto>(_jsonOptions);

        // Act
        var response = await _client.PutAsJsonAsync<UpdateCustomerRequest>($"/api/v1/customers/{createdCustomer!.Id}", null!);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetCustomer_WithOrders_IncludesPaymentCalculations()
    {
        // This test would require creating orders, which depends on OrdersController
        // For now, we verify that payment calculations are included in the response
        // A more comprehensive test can be added when OrdersController is implemented
        
        // Arrange - Create a customer
        var createRequest = new CreateCustomerRequest
        {
            FirstName = "Payment",
            LastName = "Test"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/customers", createRequest);
        var createdCustomer = await createResponse.Content.ReadFromJsonAsync<CustomerDto>(_jsonOptions);

        // Act
        var response = await _client.GetAsync($"/api/v1/customers/{createdCustomer!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var customer = await response.Content.ReadFromJsonAsync<CustomerDto>(_jsonOptions);
        customer.Should().NotBeNull();
        customer!.EstimatedPaymentDue.Should().BeGreaterThanOrEqualTo(0m);
        customer.ActualPaymentDue.Should().BeGreaterThanOrEqualTo(0m);
    }
}
