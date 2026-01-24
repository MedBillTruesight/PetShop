namespace PetShop.Api.DTOs;

/// <summary>
/// Error response structure matching the API contract.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Gets or sets the error details.
    /// </summary>
    public ErrorDetail Error { get; set; } = new();
}

/// <summary>
/// Error detail structure containing code, message, and optional details.
/// </summary>
public class ErrorDetail
{
    /// <summary>
    /// Gets or sets the machine-readable error code (e.g., "ORDER_INVALID_STATE", "VALIDATION_ERROR").
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the human-readable error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets optional additional context (field names, provided values, constraints).
    /// </summary>
    public Dictionary<string, object>? Details { get; set; }
}
