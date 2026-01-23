namespace PetShop.Application.DTOs;

/// <summary>
/// Request DTO for creating a new customer.
/// </summary>
public class CreateCustomerRequest
{
    /// <summary>
    /// Gets or sets the customer's first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer's last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer's email address.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the customer's phone number.
    /// </summary>
    public string? Phone { get; set; }
}
