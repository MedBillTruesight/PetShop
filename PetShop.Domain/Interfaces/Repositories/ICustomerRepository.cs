using PetShop.Domain.Entities;

namespace PetShop.Domain.Interfaces.Repositories;
public interface ICustomerRepository
{
    Task<Customer> CreateCustomerAsync(Customer customer);
    Task<Customer?> UpdateCustomerAsync(Customer customer);
    Task<Customer?> GetCustomerByIdAsync(Guid id);
    Task<Customer?> DeleteCustomerByIdAsync(Guid id);
    Task<List<Customer>> GetAllCustomersAsync();
    
}