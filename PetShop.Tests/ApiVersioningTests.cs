using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using PetShop.Api;
using System.Net;

namespace PetShop.Tests;

/// <summary>
/// Integration tests for API versioning configuration.
/// Tests verify that versioned endpoints are accessible and non-versioned endpoints are handled correctly.
/// </summary>
public class ApiVersioningTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ApiVersioningTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetTestEndpoint_VersionedUrl_ShouldReturnSuccess()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("API versioning is configured correctly");
        content.Should().Contain("1.0");
    }

    [Fact]
    public async Task GetTestEndpoint_NonVersionedUrl_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetTestEndpoint_ResponseHeaders_ShouldIncludeApiVersion()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().ContainKey("api-supported-versions");
        // api-deprecated-versions header is only present when there are deprecated versions
        // Since we only have v1, this header won't be present
    }

    [Fact]
    public async Task GetSwaggerEndpoint_Versioned_ShouldReturnSwaggerJson()
    {
        // Act
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Pet Shop API");
        content.Should().Contain("1.0");
    }
}
