namespace PetShop.Domain;

/// <summary>
/// Represents an order in the pet shop system.
/// This is a placeholder that will be fully implemented in P2-004.
/// </summary>
public class Order
{
    /// <summary>
    /// Gets the unique identifier for the order.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Order"/> class.
    /// </summary>
    public Order()
    {
        Id = Guid.NewGuid();
    }
}
