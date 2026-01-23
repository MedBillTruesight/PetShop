namespace PetShop.Domain;

/// <summary>
/// Represents a pet in an order within the pet shop system.
/// Pets belong to exactly one order and cannot exist independently.
/// </summary>
public class Pet
{
    /// <summary>
    /// Gets the unique identifier for the pet.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the identifier of the order this pet belongs to.
    /// </summary>
    public Guid OrderId { get; private set; }

    /// <summary>
    /// Gets the name of the pet.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the price of the pet.
    /// </summary>
    public decimal Price { get; private set; }

    /// <summary>
    /// Gets the kind/type of the pet, if provided.
    /// </summary>
    public string? Kind { get; private set; }

    /// <summary>
    /// Gets the color of the pet, if provided.
    /// </summary>
    public string? Color { get; private set; }

    /// <summary>
    /// Gets the order this pet belongs to.
    /// </summary>
    public Order Order { get; private set; } = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="Pet"/> class.
    /// </summary>
    /// <param name="orderId">The identifier of the order this pet belongs to. Cannot be empty.</param>
    /// <param name="name">The name of the pet. Cannot be null or empty.</param>
    /// <param name="price">The price of the pet. Must be greater than zero.</param>
    /// <param name="kind">The kind/type of the pet. Optional.</param>
    /// <param name="color">The color of the pet. Optional.</param>
    /// <exception cref="BusinessRuleViolationException">Thrown when required fields are missing or price is invalid.</exception>
    public Pet(Guid orderId, string name, decimal price, string? kind = null, string? color = null)
    {
        ValidateOrderId(orderId);
        ValidateName(name);
        ValidatePrice(price);

        Id = Guid.NewGuid();
        OrderId = orderId;
        Name = name;
        Price = price;
        Kind = kind;
        Color = color;
    }

    /// <summary>
    /// Updates the pet's name.
    /// </summary>
    /// <param name="name">The new name. Cannot be null or empty.</param>
    /// <exception cref="BusinessRuleViolationException">Thrown when name is null or empty.</exception>
    public void UpdateName(string name)
    {
        ValidateName(name);
        Name = name;
    }

    /// <summary>
    /// Updates the pet's price.
    /// </summary>
    /// <param name="price">The new price. Must be greater than zero.</param>
    /// <exception cref="BusinessRuleViolationException">Thrown when price is zero or negative.</exception>
    public void UpdatePrice(decimal price)
    {
        ValidatePrice(price);
        Price = price;
    }

    /// <summary>
    /// Updates the pet's kind/type.
    /// </summary>
    /// <param name="kind">The new kind/type. Optional.</param>
    public void UpdateKind(string? kind)
    {
        Kind = kind;
    }

    /// <summary>
    /// Updates the pet's color.
    /// </summary>
    /// <param name="color">The new color. Optional.</param>
    public void UpdateColor(string? color)
    {
        Color = color;
    }

    private static void ValidateOrderId(Guid orderId)
    {
        if (orderId == Guid.Empty)
        {
            throw new BusinessRuleViolationException("Order ID is required and cannot be empty.");
        }
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BusinessRuleViolationException("Pet name is required and cannot be empty.");
        }
    }

    private static void ValidatePrice(decimal price)
    {
        if (price <= 0)
        {
            throw new BusinessRuleViolationException($"Pet price must be greater than zero. Provided value: {price}");
        }
    }
}
