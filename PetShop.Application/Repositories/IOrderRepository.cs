using PetShop.Domain;

namespace PetShop.Application.Repositories;

/// <summary>
/// Repository interface for Order entity operations.
/// Provides abstraction for data access operations on orders.
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    /// Gets an order by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the order.</param>
    /// <returns>The order if found, otherwise null.</returns>
    Task<Order?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets all orders.
    /// </summary>
    /// <returns>A collection of all orders.</returns>
    Task<IEnumerable<Order>> GetAllAsync();

    /// <summary>
    /// Gets all orders for a specific customer.
    /// </summary>
    /// <param name="customerId">The unique identifier of the customer.</param>
    /// <returns>A collection of orders for the specified customer.</returns>
    Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId);

    /// <summary>
    /// Creates a new order.
    /// </summary>
    /// <param name="order">The order to create.</param>
    /// <returns>The created order.</returns>
    Task<Order> CreateAsync(Order order);

    /// <summary>
    /// Updates an existing order.
    /// </summary>
    /// <param name="order">The order to update.</param>
    /// <returns>The updated order.</returns>
    Task<Order> UpdateAsync(Order order);

    /// <summary>
    /// Deletes an order by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the order to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteAsync(Guid id);
}
