using FluentAssertions;
using PetShop.Application.DTOs;
using System.Net.Http.Json;
using System.Text.Json;

/// <summary>
/// Base test class for API integration tests.
/// Provides common HTTP client setup and utility methods.
/// </summary>
public abstract class BaseApiTest : BaseTest, IClassFixture<TestServerFixture>
{
    protected readonly TestServerFixture ServerFixture;
    protected readonly HttpClient Client;
    protected readonly JsonSerializerOptions JsonOptions;

    protected BaseApiTest(TestServerFixture serverFixture)
    {
        ServerFixture = serverFixture;
        Client = ServerFixture.CreateClientWithJsonDefaults();
        JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <summary>
    /// Creates a test customer via the API and returns the created customer.
    /// </summary>
    protected async Task<CustomerDto> CreateTestCustomerAsync(
        string firstName = "Test",
        string lastName = "Customer",
        string? email = null,
        string? phone = null)
    {
        var request = new CreateCustomerRequest
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Phone = phone
        };

        var response = await Client.PostAsJsonAsync("/api/v1/customers", request);
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

        var customer = await response.Content.ReadFromJsonAsync<CustomerDto>(JsonOptions);
        customer.Should().NotBeNull();

        return customer!;
    }

    /// <summary>
    /// Creates a test order via the API and returns the created order.
    /// </summary>
    protected async Task<OrderDto> CreateTestOrderAsync(
        Guid customerId,
        DateOnly pickupDate)
    {
        var request = new CreateOrderRequest
        {
            CustomerId = customerId,
            PickupDate = pickupDate
        };

        var response = await Client.PostAsJsonAsync("/api/v1/orders", request);
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

        var order = await response.Content.ReadFromJsonAsync<OrderDto>(JsonOptions);
        order.Should().NotBeNull();

        return order!;
    }

    /// <summary>
    /// Adds a pet to an existing order via the API.
    /// </summary>
    protected async Task<PetDto> AddPetToOrderAsync(
        Guid orderId,
        string name = "TestPet",
        decimal price = 100m)
    {
        var request = new AddPetRequest
        {
            Name = name,
            Price = price
        };

        var response = await Client.PostAsJsonAsync($"/api/v1/orders/{orderId}/pets", request);
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

        var order = await response.Content.ReadFromJsonAsync<OrderDto>(JsonOptions);
        order.Should().NotBeNull();

        return order!.Pets.Last();
    }

    /// <summary>
    /// Asserts that a response has a specific status code.
    /// </summary>
    protected static void AssertStatusCode(HttpResponseMessage response, System.Net.HttpStatusCode expectedStatus)
    {
        response.StatusCode.Should().Be(expectedStatus);
    }

    /// <summary>
    /// Asserts that a response is successful (2xx status code).
    /// </summary>
    protected static void AssertSuccess(HttpResponseMessage response)
    {
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    /// <summary>
    /// Gets the location header from a response.
    /// </summary>
    protected static string? GetLocation(HttpResponseMessage response)
    {
        return response.Headers.Location?.ToString();
    }
}
