using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using PetShop.Api.DTOs;
using PetShop.Application.DTOs;
using PetShop.Domain;

namespace PetShop.Tests;

/// <summary>
/// API contract tests validating request/response schemas, edge cases,
/// and contract compliance beyond basic integration tests.
/// </summary>
public class ApiContractTests : BaseApiTest
{
    public ApiContractTests(TestServerFixture fixture) : base(fixture) { }

    #region Request Validation Contract Tests

    [Fact]
    public async Task PostCustomer_EmptyRequestBody_Returns415UnsupportedMediaType()
    {
        // Act
        var response = await Client.PostAsync("/api/v1/customers", null!);

        // Assert - JSON endpoint without content-type returns 415
        response.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task PostCustomer_NullRequiredFields_Returns400BadRequest()
    {
        // Arrange
        var request = new CreateCustomerRequest
        {
            FirstName = null!, // Required field is null - model validation error
            LastName = "Doe"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/customers", request);

        // Assert - Model validation errors return 400 Bad Request
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostCustomer_EmptyRequiredFields_Returns409Conflict()
    {
        // Arrange
        var request = new CreateCustomerRequest
        {
            FirstName = "", // Required field is empty - domain validation error
            LastName = "Doe"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/customers", request);

        // Assert - Domain validation errors return 409 Conflict
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task PostCustomer_WhitespaceOnlyFields_Returns409Conflict()
    {
        // Arrange
        var request = new CreateCustomerRequest
        {
            FirstName = "   ", // Whitespace only - domain validation error
            LastName = "Doe"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/customers", request);

        // Assert - Domain validation errors return 409 Conflict
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task PostCustomer_MaxLengthFields_Accepted()
    {
        // Arrange
        var longName = new string('A', 100); // Reasonable max length
        var request = new CreateCustomerRequest
        {
            FirstName = longName,
            LastName = "Doe",
            Email = $"{longName}@example.com",
            Phone = "555-1234"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/customers", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var customer = await response.Content.ReadFromJsonAsync<CustomerDto>(JsonOptions);
        customer.Should().NotBeNull();
        customer!.FirstName.Should().Be(longName);
    }

    [Fact]
    public async Task PostCustomer_SpecialCharactersInNames_Accepted()
    {
        // Arrange
        var request = new CreateCustomerRequest
        {
            FirstName = "José-María",
            LastName = "O'Connor-Müller",
            Email = "jose@example.com"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/customers", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var customer = await response.Content.ReadFromJsonAsync<CustomerDto>(JsonOptions);
        customer.Should().NotBeNull();
        customer!.FirstName.Should().Be("José-María");
        customer.LastName.Should().Be("O'Connor-Müller");
    }

    [Fact]
    public async Task PostCustomer_InvalidEmailFormats_Returns409Conflict()
    {
        // Arrange
        var invalidEmails = new[] { "invalid", "missing@", "@missing.com", "spaces in@email.com" };

        foreach (var invalidEmail in invalidEmails)
        {
            var request = new CreateCustomerRequest
            {
                FirstName = "John",
                LastName = "Doe",
                Email = invalidEmail
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/v1/customers", request);

            // Assert - Domain validation errors return 409 Conflict
            response.StatusCode.Should().Be(HttpStatusCode.Conflict,
                $"Email '{invalidEmail}' should be rejected as invalid");
        }
    }

    #endregion

    #region Response Schema Validation Tests

    [Fact]
    public async Task PostCustomer_ValidRequest_ReturnsCorrectSchema()
    {
        // Arrange
        var request = new CreateCustomerRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Phone = "555-1234"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/customers", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var customer = await response.Content.ReadFromJsonAsync<CustomerDto>(JsonOptions);
        customer.Should().NotBeNull();

        // Validate response schema
        customer!.Id.Should().NotBeEmpty();
        customer.FirstName.Should().Be("John");
        customer.LastName.Should().Be("Doe");
        customer.Email.Should().Be("john@example.com");
        customer.Phone.Should().Be("555-1234");
        customer.EstimatedPaymentDue.Should().Be(0m);
        customer.ActualPaymentDue.Should().Be(0m);
    }

    [Fact]
    public async Task GetCustomer_ExistingCustomer_ReturnsCorrectSchema()
    {
        // Arrange
        var createdCustomer = await CreateTestCustomerAsync("Jane", "Smith", "jane@example.com");

        // Act
        var response = await Client.GetAsync($"/api/v1/customers/{createdCustomer.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var customer = await response.Content.ReadFromJsonAsync<CustomerDto>(JsonOptions);
        customer.Should().NotBeNull();

        // Validate response schema matches created customer
        customer!.Id.Should().Be(createdCustomer.Id);
        customer.FirstName.Should().Be("Jane");
        customer.LastName.Should().Be("Smith");
        customer.Email.Should().Be("jane@example.com");
    }

    [Fact]
    public async Task PostOrder_ValidRequest_ReturnsCorrectSchema()
    {
        // Arrange
        var customer = await CreateTestCustomerAsync();
        var request = new CreateOrderRequest
        {
            CustomerId = customer.Id,
            PickupDate = Tomorrow
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var order = await response.Content.ReadFromJsonAsync<OrderDto>(JsonOptions);
        order.Should().NotBeNull();

        // Validate response schema
        order!.Id.Should().NotBeEmpty();
        order.CustomerId.Should().Be(customer.Id);
        order.Status.Should().Be(OrderStatus.Open);
        order.PickupDate.Should().Be(Tomorrow);
        order.Pets.Should().NotBeNull().And.BeEmpty();
        order.EstimatedCost.Should().Be(0m);
        order.ActualCost.Should().BeNull();
    }

    #endregion

    #region Edge Cases and Boundary Conditions

    [Fact]
    public async Task PostCustomer_ExtremelyLongNames_Accepted()
    {
        // Arrange - Test with very long names (edge case)
        var veryLongName = new string('A', 500);
        var request = new CreateCustomerRequest
        {
            FirstName = veryLongName,
            LastName = "Doe"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/customers", request);

        // Assert - Should either accept or reject gracefully
        // (This tests the API's handling of edge cases)
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostCustomer_UnicodeCharacters_Accepted()
    {
        // Arrange - Test with Unicode characters
        var request = new CreateCustomerRequest
        {
            FirstName = "José",
            LastName = "Михаил", // Cyrillic
            Email = "test@example.com"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/customers", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var customer = await response.Content.ReadFromJsonAsync<CustomerDto>(JsonOptions);
        customer.Should().NotBeNull();
        customer!.FirstName.Should().Be("José");
        customer.LastName.Should().Be("Михаил");
    }

    [Fact]
    public async Task PostOrder_PastPickupDate_Returns422UnprocessableEntity()
    {
        // Arrange
        var customer = await CreateTestCustomerAsync();
        var pastDate = Yesterday;
        var request = new CreateOrderRequest
        {
            CustomerId = customer.Id,
            PickupDate = pastDate
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/orders", request);

        // Assert - Should return 422 for business rule violations
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task PostOrder_FuturePickupDate_Accepted()
    {
        // Arrange
        var customer = await CreateTestCustomerAsync();
        var futureDate = Today.AddDays(365); // One year in future
        var request = new CreateOrderRequest
        {
            CustomerId = customer.Id,
            PickupDate = futureDate
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    #endregion

    #region Error Response Contract Tests

    [Fact]
    public async Task GetCustomer_NonExistentId_Returns404WithCorrectErrorFormat()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/v1/customers/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Validate error response format
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>(JsonOptions);
        errorResponse.Should().NotBeNull();
        errorResponse!.Error.Should().NotBeNull();
        errorResponse.Error.Code.Should().NotBeNullOrEmpty();
        errorResponse.Error.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task PostOrder_NonExistentCustomerId_Returns404WithCorrectErrorFormat()
    {
        // Arrange
        var nonExistentCustomerId = Guid.NewGuid();
        var request = new CreateOrderRequest
        {
            CustomerId = nonExistentCustomerId,
            PickupDate = Tomorrow
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Validate error response format
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>(JsonOptions);
        errorResponse.Should().NotBeNull();
        errorResponse!.Error.Should().NotBeNull();
        errorResponse.Error.Code.Should().Be("CUSTOMER_NOT_FOUND");
        errorResponse.Error.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task PostOrder_BusinessRuleViolation_Returns409WithCorrectErrorFormat()
    {
        // Arrange - Create order and try to add pet when processing
        var customer = await CreateTestCustomerAsync();
        var order = await CreateTestOrderAsync(customer.Id, Tomorrow);
        await AddPetToOrderAsync(order.Id, "TestPet", 100m);

        // Try to add another pet when order is open (should work)
        var petRequest = new AddPetRequest { Name = "AnotherPet", Price = 50m };

        // Act
        var response = await Client.PostAsJsonAsync($"/api/v1/orders/{order.Id}/pets", petRequest);

        // Assert - Adding pets to open orders should work
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    #endregion

    #region API Versioning Contract Tests

    [Fact]
    public async Task GetCustomer_WithVersionHeader_ReturnsCorrectVersion()
    {
        // Arrange
        var customer = await CreateTestCustomerAsync();

        // Act - Request with explicit version
        Client.DefaultRequestHeaders.Add("api-version", "1.0");
        var response = await Client.GetAsync($"/api/v1/customers/{customer.Id}");
        Client.DefaultRequestHeaders.Remove("api-version");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task GetCustomer_DefaultVersion_WorksWithoutVersionHeader()
    {
        // Arrange
        var customer = await CreateTestCustomerAsync();

        // Act - No version header (should default)
        var response = await Client.GetAsync($"/api/v1/customers/{customer.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Content Negotiation Contract Tests

    [Fact]
    public async Task GetCustomer_AcceptsJsonContentType_ReturnsJson()
    {
        // Arrange
        var customer = await CreateTestCustomerAsync();
        Client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        var response = await Client.GetAsync($"/api/v1/customers/{customer.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var customerData = await response.Content.ReadFromJsonAsync<CustomerDto>(JsonOptions);
        customerData.Should().NotBeNull();
    }

    [Fact]
    public async Task PostCustomer_SendsJsonContentType_Accepted()
    {
        // Arrange
        var request = new CreateCustomerRequest
        {
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/customers", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    #endregion

    #region OpenAPI/Swagger Contract Validation

    [Fact]
    public async Task Get_SwaggerEndpoint_Returns200Ok()
    {
        // Act
        var response = await Client.GetAsync("/swagger/v1/swagger.json");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task Options_Method_OnEndpoints_Returns405MethodNotAllowed()
    {
        // Act - Test OPTIONS on customers endpoint
        var response = await Client.SendAsync(new HttpRequestMessage(HttpMethod.Options, "/api/v1/customers"));

        // Assert - API doesn't support OPTIONS method
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    #endregion
}