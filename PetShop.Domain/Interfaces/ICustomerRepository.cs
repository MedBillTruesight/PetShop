using PetShop.Domain.Entities;

namespace PetShop.Domain.Interfaces;

public interface ICustomerRepository
{
    Task<Customer> CreateCustomerAsync(Customer customer);
    Task<Customer?> UpdateCustomerAsync(Customer customer);
    Task<Customer?> GetCustomerByIdAsync(int id);
    Task<Customer?> DeleteCustomerByIdAsync(int id);
    Task<List<Customer>> GetAllCustomersAsync();
    
}