using PetShop.Domain;

namespace PetShop.Application.DTOs;

/// <summary>
/// Response DTO for order operations.
/// Includes cost calculations based on order status.
/// </summary>
public class OrderDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the order.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the customer who placed this order.
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the current status of the order.
    /// </summary>
    public OrderStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the scheduled pickup date for the order.
    /// </summary>
    public DateOnly PickupDate { get; set; }

    /// <summary>
    /// Gets or sets the collection of pets in this order.
    /// </summary>
    public ICollection<PetDto> Pets { get; set; } = new List<PetDto>();

    /// <summary>
    /// Gets or sets the estimated cost of the order.
    /// Calculated as sum of pet prices. Returned when status is Open or Processing.
    /// Null when status is Delivered.
    /// </summary>
    public decimal? EstimatedCost { get; set; }

    /// <summary>
    /// Gets or sets the actual cost of the order.
    /// Persisted value when status is Delivered. Null when status is not Delivered.
    /// </summary>
    public decimal? ActualCost { get; set; }
}
