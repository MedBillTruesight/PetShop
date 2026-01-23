namespace PetShop.Application.DTOs;

/// <summary>
/// Request DTO for creating a new order.
/// </summary>
public class CreateOrderRequest
{
    /// <summary>
    /// Gets or sets the unique identifier of the customer placing the order.
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the scheduled pickup date for the order.
    /// Must be today or in the future.
    /// </summary>
    public DateOnly PickupDate { get; set; }
}
