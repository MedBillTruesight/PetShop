using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using PetShop.Api;
using PetShop.Api.DTOs;
using System.Net;
using System.Text.Json;

namespace PetShop.Tests;

/// <summary>
/// Integration tests for global error handling middleware.
/// Tests verify that exceptions are properly mapped to HTTP status codes and error response format.
/// </summary>
public class ErrorHandlingTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public ErrorHandlingTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    [Fact]
    public async Task HandleException_KeyNotFoundException_Returns404WithCorrectErrorCode()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/error-test/not-found");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var content = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content, _jsonOptions);

        errorResponse.Should().NotBeNull();
        errorResponse!.Error.Should().NotBeNull();
        errorResponse.Error.Code.Should().Be("CUSTOMER_NOT_FOUND");
        errorResponse.Error.Message.Should().Contain("Customer not found");
    }

    [Fact]
    public async Task HandleException_InvalidOrderStateException_Returns409WithCorrectErrorCode()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/error-test/invalid-state");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var content = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content, _jsonOptions);

        errorResponse.Should().NotBeNull();
        errorResponse!.Error.Should().NotBeNull();
        errorResponse.Error.Code.Should().Be("ORDER_INVALID_STATE");
        errorResponse.Error.Message.Should().Contain("Cannot transition order");
    }

    [Fact]
    public async Task HandleException_BusinessRuleViolationException_Returns409ForConflict()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/error-test/business-rule-conflict");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var content = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content, _jsonOptions);

        errorResponse.Should().NotBeNull();
        errorResponse!.Error.Should().NotBeNull();
        errorResponse.Error.Code.Should().Be("BUSINESS_RULE_VIOLATION");
        errorResponse.Error.Message.Should().Contain("cannot exceed");
    }

    [Fact]
    public async Task HandleException_BusinessRuleViolationException_Returns422ForUnprocessable()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/error-test/business-rule-unprocessable");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var content = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content, _jsonOptions);

        errorResponse.Should().NotBeNull();
        errorResponse!.Error.Should().NotBeNull();
        errorResponse.Error.Code.Should().Be("BUSINESS_RULE_VIOLATION");
        errorResponse.Error.Message.Should().Contain("Pickup date");
    }

    [Fact]
    public async Task HandleException_ArgumentNullException_Returns400WithValidationError()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/error-test/argument-null");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content, _jsonOptions);

        errorResponse.Should().NotBeNull();
        errorResponse!.Error.Should().NotBeNull();
        errorResponse.Error.Code.Should().Be("VALIDATION_ERROR");
        errorResponse.Error.Message.Should().Contain("cannot be null");
        errorResponse.Error.Details.Should().NotBeNull();
        errorResponse.Error.Details!.Should().ContainKey("parameter");
    }

    [Fact]
    public async Task HandleException_ArgumentException_Returns400WithValidationError()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/error-test/argument");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content, _jsonOptions);

        errorResponse.Should().NotBeNull();
        errorResponse!.Error.Should().NotBeNull();
        errorResponse.Error.Code.Should().Be("VALIDATION_ERROR");
        errorResponse.Error.Message.Should().Contain("Invalid argument");
        errorResponse.Error.Details.Should().NotBeNull();
        errorResponse.Error.Details!.Should().ContainKey("parameter");
    }

    [Fact]
    public async Task HandleException_UnhandledException_Returns500WithGenericMessage()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/error-test/unhandled");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content, _jsonOptions);

        errorResponse.Should().NotBeNull();
        errorResponse!.Error.Should().NotBeNull();
        errorResponse.Error.Code.Should().Be("INTERNAL_ERROR");
        errorResponse.Error.Message.Should().Be("An unexpected error occurred. Please try again later.");
        errorResponse.Error.Details.Should().BeNull();
    }

    [Fact]
    public async Task HandleException_ErrorResponseFormat_MatchesApiContract()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/error-test/not-found");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var content = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content, _jsonOptions);

        // Verify structure matches API contract
        errorResponse.Should().NotBeNull();
        errorResponse!.Error.Should().NotBeNull();
        errorResponse.Error.Code.Should().NotBeNullOrEmpty();
        errorResponse.Error.Message.Should().NotBeNullOrEmpty();
        // Details is optional, so we don't require it
    }
}
