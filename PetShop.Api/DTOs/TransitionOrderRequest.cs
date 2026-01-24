using System.Text.Json.Serialization;
using PetShop.Domain;

namespace PetShop.Api.DTOs;

/// <summary>
/// Request DTO for transitioning an order to a new status.
/// Matches API contract: POST /api/v1/orders/{id}/transition body uses "targetStatus".
/// </summary>
public class TransitionOrderRequest
{
    /// <summary>
    /// Gets or sets the target status for the transition.
    /// Valid transitions: Open → Processing (1), Processing → Delivered (2).
    /// </summary>
    [JsonPropertyName("targetStatus")]
    public OrderStatus Status { get; set; }
}
