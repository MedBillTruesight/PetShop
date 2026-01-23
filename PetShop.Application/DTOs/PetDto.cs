namespace PetShop.Application.DTOs;

/// <summary>
/// Response DTO for pet information.
/// Pets exist only within orders and are not top-level resources.
/// </summary>
public class PetDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the pet.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the pet.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the price of the pet.
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
