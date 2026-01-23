namespace PetShop.Domain;

/// <summary>
/// Represents the status of an order in the system.
/// Orders progress through: Open → Processing → Delivered
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// Initial state when an order is created. Order can be modified freely.
    /// </summary>
    Open,

    /// <summary>
    /// Order has been confirmed and is being prepared. Limited modifications allowed.
    /// </summary>
    Processing,

    /// <summary>
    /// Order has been completed. Order is immutable.
    /// </summary>
    Delivered
}
