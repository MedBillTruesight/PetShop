using PetShop.Application.DTOs;

namespace PetShop.Application.Interfaces.Services;
public interface ICustomerService
{
    Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto createCustomerDto);
    Task<CustomerDto?> GetCustomerAsync(Guid id);
    Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();
    Task<CustomerDto?> UpdateCustomerAsync(Guid id, UpdateCustomerDto updateCustomerDto);
    Task<CustomerDto?> DeleteCustomerAsync(Guid id);
}