using Microsoft.AspNetCore.Mvc;

namespace PetShop.Api.Controllers;

/// <summary>
/// Test controller to verify API versioning configuration.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/test")]
public class TestController : ControllerBase
{
    /// <summary>
    /// Test endpoint to verify API versioning is working correctly.
    /// </summary>
    /// <returns>A simple response indicating the API version</returns>
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            message = "API versioning is configured correctly",
            version = "1.0",
            timestamp = DateTime.UtcNow
        });
    }
}
