namespace PetShop.Application.DTOs;

/// <summary>
/// Request DTO for updating an existing order.
/// Only pickup date can be updated (status-aware restrictions apply).
/// </summary>
public class UpdateOrderRequest
{
    /// <summary>
    /// Gets or sets the new scheduled pickup date for the order.
    /// Must be today or in the future.
    /// </summary>
    public DateOnly PickupDate { get; set; }
}
