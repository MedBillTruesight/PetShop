using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using PetShop.Api.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Reflection;

namespace PetShop.Tests;

/// <summary>
/// Unit tests for SwaggerExampleOperationFilter.
/// Tests verify that request and response examples are correctly added to Swagger documentation.
/// </summary>
public class SwaggerExampleOperationFilterTests
{
    private readonly SwaggerExampleOperationFilter _filter;

    public SwaggerExampleOperationFilterTests()
    {
        _filter = new SwaggerExampleOperationFilter();
    }

    private class TestSchemaGenerator : ISchemaGenerator
    {
        public OpenApiSchema GenerateSchema(Type type, SchemaRepository schemaRepository)
        {
            return new OpenApiSchema();
        }

        public OpenApiSchema GenerateSchema(Type modelType, SchemaRepository schemaRepository, MemberInfo memberInfo = null, ParameterInfo parameterInfo = null, ApiParameterRouteInfo routeInfo = null)
        {
            return new OpenApiSchema();
        }
    }

    #region Request Body Examples Tests

    [Theory]
    [InlineData("api/v1/customers", "POST")]
    [InlineData("api/v{version}/customers", "POST")]
    public void Apply_AddsCreateCustomerRequestExample(string path, string method)
    {
        // Arrange
        var operation = CreateOperationWithRequestBody();
        operation.Responses = CreateErrorResponses(); // Add error responses to prevent null ref

        var context = CreateOperationFilterContext(path, method);

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.RequestBody!.Content["application/json"].Example.Should().NotBeNull();
        var example = operation.RequestBody.Content["application/json"].Example as OpenApiObject;
        example.Should().NotBeNull();
        (example!["firstName"] as OpenApiString)!.Value.Should().Be("Jane");
        (example["lastName"] as OpenApiString)!.Value.Should().Be("Doe");
        (example["email"] as OpenApiString)!.Value.Should().Be("jane.doe@example.com");
        (example["phone"] as OpenApiString)!.Value.Should().Be("555-1234");
    }

    [Theory]
    [InlineData("api/v1/customers/{id}", "PUT")]
    [InlineData("api/v{version}/customers/{id}", "PUT")]
    public void Apply_AddsUpdateCustomerRequestExample(string path, string method)
    {
        // Arrange
        var operation = CreateOperationWithRequestBody();
        operation.Responses = CreateErrorResponses();

        var context = CreateOperationFilterContext(path, method);

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.RequestBody!.Content["application/json"].Example.Should().NotBeNull();
        var example = operation.RequestBody.Content["application/json"].Example as OpenApiObject;
        example.Should().NotBeNull();
        (example!["firstName"] as OpenApiString)!.Value.Should().Be("Jane");
        (example["lastName"] as OpenApiString)!.Value.Should().Be("Smith");
        (example["email"] as OpenApiString)!.Value.Should().Be("jane.smith@example.com");
        (example["phone"] as OpenApiString)!.Value.Should().Be("555-5678");
    }

    [Theory]
    [InlineData("api/v1/orders", "POST")]
    [InlineData("api/v{version}/orders", "POST")]
    public void Apply_AddsCreateOrderRequestExample(string path, string method)
    {
        // Arrange
        var operation = CreateOperationWithRequestBody();
        operation.Responses = CreateErrorResponses();

        var context = CreateOperationFilterContext(path, method);

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.RequestBody!.Content["application/json"].Example.Should().NotBeNull();
        var example = operation.RequestBody.Content["application/json"].Example as OpenApiObject;
        example.Should().NotBeNull();
        (example!["customerId"] as OpenApiString)!.Value.Should().Be("3fa85f64-5717-4562-b3fc-2c963f66afa6");
        (example["pickupDate"] as OpenApiString)!.Value.Should().Be(DateOnly.FromDateTime(DateTime.Today.AddDays(1)).ToString("yyyy-MM-dd"));
    }

    [Theory]
    [InlineData("api/v1/orders/{id}", "PATCH")]
    [InlineData("api/v{version}/orders/{id}", "PATCH")]
    public void Apply_AddsUpdateOrderRequestExample(string path, string method)
    {
        // Arrange
        var operation = CreateOperationWithRequestBody();
        operation.Responses = CreateErrorResponses();

        var context = CreateOperationFilterContext(path, method);

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.RequestBody!.Content["application/json"].Example.Should().NotBeNull();
        var example = operation.RequestBody.Content["application/json"].Example as OpenApiObject;
        example.Should().NotBeNull();
        (example!["pickupDate"] as OpenApiString)!.Value.Should().Be(DateOnly.FromDateTime(DateTime.Today.AddDays(2)).ToString("yyyy-MM-dd"));
    }

    [Theory]
    [InlineData("api/v1/orders/{id}/transition", "POST")]
    [InlineData("api/v{version}/orders/{id}/transition", "POST")]
    public void Apply_AddsTransitionOrderRequestExample(string path, string method)
    {
        // Arrange
        var operation = CreateOperationWithRequestBody();
        operation.Responses = CreateErrorResponses();

        var context = CreateOperationFilterContext(path, method);

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.RequestBody!.Content["application/json"].Example.Should().NotBeNull();
        var example = operation.RequestBody.Content["application/json"].Example as OpenApiObject;
        example.Should().NotBeNull();
        (example!["targetStatus"] as OpenApiInteger)!.Value.Should().Be(1);
    }

    [Theory]
    [InlineData("api/v1/orders/{id}/pets", "POST")]
    [InlineData("api/v{version}/orders/{id}/pets", "POST")]
    public void Apply_AddsAddPetRequestExample(string path, string method)
    {
        // Arrange
        var operation = CreateOperationWithRequestBody();
        operation.Responses = CreateErrorResponses();

        var context = CreateOperationFilterContext(path, method);

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.RequestBody!.Content["application/json"].Example.Should().NotBeNull();
        var example = operation.RequestBody.Content["application/json"].Example as OpenApiObject;
        example.Should().NotBeNull();
        (example!["name"] as OpenApiString)!.Value.Should().Be("Fluffy");
        (example["price"] as OpenApiDouble)!.Value.Should().Be(100.50);
        (example["kind"] as OpenApiString)!.Value.Should().Be("Cat");
        (example["color"] as OpenApiString)!.Value.Should().Be("White");
    }

    [Fact]
    public void Apply_NoMatchingPath_DoesNotAddRequestBodyExample()
    {
        // Arrange
        var operation = CreateOperationWithRequestBody();
        operation.Responses = CreateErrorResponses();

        var context = CreateOperationFilterContext("api/v1/unknown", "GET");

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.RequestBody!.Content["application/json"].Example.Should().BeNull();
    }

    [Fact]
    public void Apply_NoRequestBody_DoesNotThrow()
    {
        // Arrange
        var operation = new OpenApiOperation();
        operation.Responses = CreateErrorResponses();

        var context = CreateOperationFilterContext("api/v1/customers", "POST");

        // Act
        _filter.Apply(operation, context);

        // Assert - Should not throw
        operation.RequestBody.Should().BeNull();
    }

    #endregion

    #region Success Response Examples Tests

    [Theory]
    [InlineData("api/v1/customers", "POST")]
    [InlineData("api/v{version}/customers", "POST")]
    public void Apply_AddsCreateCustomerResponseExample(string path, string method)
    {
        // Arrange
        var operation = new OpenApiOperation();
        operation.Responses = CreateErrorResponses();
        var response = new OpenApiResponse();
        response.Content = new Dictionary<string, OpenApiMediaType>
        {
            ["application/json"] = new OpenApiMediaType()
        };
        operation.Responses["201"] = response;

        var context = CreateOperationFilterContext(path, method);

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Responses["201"].Content["application/json"].Example.Should().NotBeNull();
        var example = operation.Responses["201"].Content["application/json"].Example as OpenApiObject;
        example.Should().NotBeNull();
        (example!["id"] as OpenApiString)!.Value.Should().Be("3fa85f64-5717-4562-b3fc-2c963f66afa6");
        (example["firstName"] as OpenApiString)!.Value.Should().Be("Jane");
        (example["lastName"] as OpenApiString)!.Value.Should().Be("Doe");
        (example["estimatedPaymentDue"] as OpenApiDouble)!.Value.Should().Be(0.0);
        (example["actualPaymentDue"] as OpenApiDouble)!.Value.Should().Be(0.0);
    }

    [Theory]
    [InlineData("api/v1/customers/{id}", "GET")]
    [InlineData("api/v{version}/customers/{id}", "GET")]
    public void Apply_AddsGetCustomerResponseExample(string path, string method)
    {
        // Arrange
        var operation = new OpenApiOperation();
        operation.Responses = CreateErrorResponses();
        var response = new OpenApiResponse();
        response.Content = new Dictionary<string, OpenApiMediaType>
        {
            ["application/json"] = new OpenApiMediaType()
        };
        operation.Responses["200"] = response;

        var context = CreateOperationFilterContext(path, method);

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Responses["200"].Content["application/json"].Example.Should().NotBeNull();
        var example = operation.Responses["200"].Content["application/json"].Example as OpenApiObject;
        example.Should().NotBeNull();
        (example!["id"] as OpenApiString)!.Value.Should().Be("3fa85f64-5717-4562-b3fc-2c963f66afa6");
        (example["firstName"] as OpenApiString)!.Value.Should().Be("Jane");
        (example["lastName"] as OpenApiString)!.Value.Should().Be("Doe");
        (example["estimatedPaymentDue"] as OpenApiDouble)!.Value.Should().Be(0.0);
        (example["actualPaymentDue"] as OpenApiDouble)!.Value.Should().Be(0.0);
    }

    [Theory]
    [InlineData("api/v1/orders", "POST")]
    [InlineData("api/v{version}/orders", "POST")]
    public void Apply_AddsCreateOrderResponseExample(string path, string method)
    {
        // Arrange
        var operation = new OpenApiOperation();
        operation.Responses = CreateErrorResponses();
        var response = new OpenApiResponse();
        response.Content = new Dictionary<string, OpenApiMediaType>
        {
            ["application/json"] = new OpenApiMediaType()
        };
        operation.Responses["201"] = response;

        var context = CreateOperationFilterContext(path, method);

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Responses["201"].Content["application/json"].Example.Should().NotBeNull();
        var example = operation.Responses["201"].Content["application/json"].Example as OpenApiObject;
        example.Should().NotBeNull();
        (example!["id"] as OpenApiString)!.Value.Should().Be("3fa85f64-5717-4562-b3fc-2c963f66afa6");
        (example["customerId"] as OpenApiString)!.Value.Should().Be("7a85f64-5717-4562-b3fc-2c963f66afa7");
        (example["status"] as OpenApiInteger)!.Value.Should().Be(0);
        (example["pickupDate"] as OpenApiString)!.Value.Should().Be(DateOnly.FromDateTime(DateTime.Today.AddDays(1)).ToString("yyyy-MM-dd"));
        example["pets"].Should().BeOfType<OpenApiArray>();
        (example["estimatedCost"] as OpenApiDouble)!.Value.Should().Be(0.0);
    }

    [Theory]
    [InlineData("api/v1/customers", "GET")]
    [InlineData("api/v{version}/customers", "GET")]
    public void Apply_AddsGetCustomersResponseExample(string path, string method)
    {
        // Arrange
        var operation = new OpenApiOperation();
        operation.Responses = CreateErrorResponses();
        var response = new OpenApiResponse();
        response.Content = new Dictionary<string, OpenApiMediaType>
        {
            ["application/json"] = new OpenApiMediaType()
        };
        operation.Responses["200"] = response;

        var context = CreateOperationFilterContext(path, method);

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Responses["200"].Content["application/json"].Example.Should().NotBeNull();
        var example = operation.Responses["200"].Content["application/json"].Example as OpenApiArray;
        example.Should().NotBeNull();
        example!.Count.Should().Be(2);

        var firstCustomer = example[0] as OpenApiObject;
        firstCustomer.Should().NotBeNull();
        (firstCustomer!["firstName"] as OpenApiString)!.Value.Should().Be("Jane");
        (firstCustomer["estimatedPaymentDue"] as OpenApiDouble)!.Value.Should().Be(0.0);

        var secondCustomer = example[1] as OpenApiObject;
        secondCustomer.Should().NotBeNull();
        (secondCustomer!["firstName"] as OpenApiString)!.Value.Should().Be("Jane");
        (secondCustomer["estimatedPaymentDue"] as OpenApiDouble)!.Value.Should().Be(150.00);
        (secondCustomer["actualPaymentDue"] as OpenApiDouble)!.Value.Should().Be(250.00);
    }

    [Theory]
    [InlineData("api/v1/orders", "GET")]
    [InlineData("api/v{version}/orders", "GET")]
    public void Apply_AddsGetOrdersResponseExample(string path, string method)
    {
        // Arrange
        var operation = new OpenApiOperation();
        operation.Responses = CreateErrorResponses();
        var response = new OpenApiResponse();
        response.Content = new Dictionary<string, OpenApiMediaType>
        {
            ["application/json"] = new OpenApiMediaType()
        };
        operation.Responses["200"] = response;

        var context = CreateOperationFilterContext(path, method);

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Responses["200"].Content["application/json"].Example.Should().NotBeNull();
        var example = operation.Responses["200"].Content["application/json"].Example as OpenApiArray;
        example.Should().NotBeNull();
        example!.Count.Should().Be(2);

        var firstOrder = example[0] as OpenApiObject;
        firstOrder.Should().NotBeNull();
        (firstOrder!["status"] as OpenApiInteger)!.Value.Should().Be(0);
        (firstOrder["estimatedCost"] as OpenApiDouble)!.Value.Should().Be(0.0);

        var secondOrder = example[1] as OpenApiObject;
        secondOrder.Should().NotBeNull();
        (secondOrder!["status"] as OpenApiInteger)!.Value.Should().Be(2);
        (secondOrder["actualCost"] as OpenApiDouble)!.Value.Should().Be(150.00);
    }

    [Theory]
    [InlineData("api/v1/customers/{id}/orders", "GET")]
    [InlineData("api/v{version}/customers/{id}/orders", "GET")]
    public void Apply_AddsGetCustomerOrdersResponseExample(string path, string method)
    {
        // Arrange
        var operation = new OpenApiOperation();
        operation.Responses = CreateErrorResponses();
        var response = new OpenApiResponse();
        response.Content = new Dictionary<string, OpenApiMediaType>
        {
            ["application/json"] = new OpenApiMediaType()
        };
        operation.Responses["200"] = response;

        var context = CreateOperationFilterContext(path, method);

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Responses["200"].Content["application/json"].Example.Should().NotBeNull();
        var example = operation.Responses["200"].Content["application/json"].Example as OpenApiArray;
        example.Should().NotBeNull();
        example!.Count.Should().Be(2);

        var firstOrder = example[0] as OpenApiObject;
        firstOrder.Should().NotBeNull();
        (firstOrder!["status"] as OpenApiInteger)!.Value.Should().Be(0);
        (firstOrder["estimatedCost"] as OpenApiDouble)!.Value.Should().Be(0.0);

        var secondOrder = example[1] as OpenApiObject;
        secondOrder.Should().NotBeNull();
        (secondOrder!["status"] as OpenApiInteger)!.Value.Should().Be(2);
        (secondOrder["actualCost"] as OpenApiDouble)!.Value.Should().Be(0.00);
    }

    [Theory]
    [InlineData("api/v1/orders/{id}/pets", "POST")]
    [InlineData("api/v{version}/orders/{id}/pets", "POST")]
    public void Apply_AddsAddPetResponseExample(string path, string method)
    {
        // Arrange
        var operation = new OpenApiOperation();
        operation.Responses = CreateErrorResponses();
        var response = new OpenApiResponse();
        response.Content = new Dictionary<string, OpenApiMediaType>
        {
            ["application/json"] = new OpenApiMediaType()
        };
        operation.Responses["201"] = response;

        var context = CreateOperationFilterContext(path, method);

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Responses["201"].Content["application/json"].Example.Should().NotBeNull();
        var example = operation.Responses["201"].Content["application/json"].Example as OpenApiObject;
        example.Should().NotBeNull();
        (example!["id"] as OpenApiString)!.Value.Should().Be("3fa85f64-5717-4562-b3fc-2c963f66afa6");
        (example["status"] as OpenApiInteger)!.Value.Should().Be(0);
        (example["estimatedCost"] as OpenApiDouble)!.Value.Should().Be(100.50);

        var pets = example["pets"] as OpenApiArray;
        pets.Should().NotBeNull();
        pets!.Count.Should().Be(1);

        var pet = pets[0] as OpenApiObject;
        pet.Should().NotBeNull();
        (pet!["name"] as OpenApiString)!.Value.Should().Be("Fluffy");
        (pet["price"] as OpenApiDouble)!.Value.Should().Be(100.50);
        (pet["kind"] as OpenApiString)!.Value.Should().Be("Cat");
        (pet["color"] as OpenApiString)!.Value.Should().Be("White");
    }

    [Fact]
    public void Apply_NoMatchingResponse_DoesNotAddExample()
    {
        // Arrange
        var operation = new OpenApiOperation();
        operation.Responses = CreateErrorResponses();
        var response = new OpenApiResponse();
        response.Content = new Dictionary<string, OpenApiMediaType>
        {
            ["application/json"] = new OpenApiMediaType()
        };
        operation.Responses["200"] = response;

        var context = CreateOperationFilterContext("api/v1/unknown", "GET");

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Responses["200"].Content["application/json"].Example.Should().BeNull();
    }

    #endregion

    #region Error Response Examples Tests

    [Fact]
    public void Apply_AddsErrorResponseExamples()
    {
        // Arrange
        var operation = new OpenApiOperation();
        operation.Responses = CreateErrorResponses();

        var context = CreateOperationFilterContext("api/v1/customers", "POST");

        // Act
        _filter.Apply(operation, context);

        // Assert that error examples are added
        foreach (var kvp in operation.Responses)
        {
            kvp.Value.Content["application/json"].Example.Should().NotBeNull();
        }
    }

    #endregion

    #region Helper Methods

    private static OpenApiOperation CreateOperationWithRequestBody()
    {
        var operation = new OpenApiOperation();
        var requestBody = new OpenApiRequestBody
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new OpenApiMediaType()
            }
        };
        operation.RequestBody = requestBody;
        return operation;
    }

    private static OpenApiResponses CreateErrorResponses()
    {
        var responses = new OpenApiResponses();
        var errorStatuses = new[] { "400", "404", "409", "422", "500" };

        foreach (var status in errorStatuses)
        {
            responses[status] = new OpenApiResponse
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType()
                }
            };
        }

        return responses;
    }

    private static OperationFilterContext CreateOperationFilterContext(string path, string method)
    {
        var apiDescription = new ApiDescription
        {
            RelativePath = path,
            HttpMethod = method
        };

        var schemaGenerator = new TestSchemaGenerator();
        var schemaRepository = new SchemaRepository();

        return new OperationFilterContext(
            apiDescription,
            schemaGenerator,
            schemaRepository,
            null!  // methodInfo
        );
    }

    #endregion
}
