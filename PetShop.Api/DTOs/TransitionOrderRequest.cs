using PetShop.Domain;

namespace PetShop.Api.DTOs;

/// <summary>
/// Request DTO for transitioning an order to a new status.
/// </summary>
public class TransitionOrderRequest
{
    /// <summary>
    /// Gets or sets the target status for the transition.
    /// Valid transitions: Open → Processing, Processing → Delivered
    /// </summary>
    public OrderStatus Status { get; set; }
}
