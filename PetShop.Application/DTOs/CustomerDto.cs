namespace PetShop.Application.DTOs;

/// <summary>
/// Response DTO for customer operations.
/// Includes calculated payment due amounts.
/// </summary>
public class CustomerDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the customer.
    /// </summary>
    public Guid Id { get; set; }

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

    /// <summary>
    /// Gets or sets the estimated payment due.
    /// Calculated as sum of estimated costs from orders with status Open or Processing.
    /// </summary>
    public decimal EstimatedPaymentDue { get; set; }

    /// <summary>
    /// Gets or sets the actual payment due.
    /// Calculated as sum of actual costs from orders with status Delivered.
    /// </summary>
    public decimal ActualPaymentDue { get; set; }
}
