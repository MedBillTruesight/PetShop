namespace PetShop.Application.DTOs;

/// <summary>
/// Request DTO for adding a pet to an order.
/// </summary>
public class AddPetRequest
{
    /// <summary>
    /// Gets or sets the name of the pet.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the price of the pet.
    /// Must be greater than zero.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the kind/type of the pet.
    /// </summary>
    public string? Kind { get; set; }

    /// <summary>
    /// Gets or sets the color of the pet.
    /// </summary>
    public string? Color { get; set; }
}
