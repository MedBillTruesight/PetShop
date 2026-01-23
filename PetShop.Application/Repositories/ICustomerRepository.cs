using PetShop.Domain;

namespace PetShop.Application.Repositories;

/// <summary>
/// Repository interface for Customer entity operations.
/// Provides abstraction for data access operations on customers.
/// </summary>
public interface ICustomerRepository
{
    /// <summary>
    /// Gets a customer by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the customer.</param>
    /// <returns>The customer if found, otherwise null.</returns>
    Task<Customer?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets all customers.
    /// </summary>
    /// <returns>A collection of all customers.</returns>
    Task<IEnumerable<Customer>> GetAllAsync();

    /// <summary>
    /// Creates a new customer.
    /// </summary>
    /// <param name="customer">The customer to create.</param>
    /// <returns>The created customer.</returns>
    Task<Customer> CreateAsync(Customer customer);

    /// <summary>
    /// Updates an existing customer.
    /// </summary>
    /// <param name="customer">The customer to update.</param>
    /// <returns>The updated customer.</returns>
    Task<Customer> UpdateAsync(Customer customer);

    /// <summary>
    /// Deletes a customer by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the customer to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteAsync(Guid id);
}
