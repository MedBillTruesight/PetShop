using PetShop.Application.DTOs;

namespace PetShop.Application.Features.Customer.Interfaces;

public interface ICustomerService
{
	Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto createCustomerDto);
	
	Task<CustomerDto?> GetCustomerAsync(int id);
	Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();
	Task<CustomerDto?> UpdateCustomerAsync(int id, UpdateCustomerDto updateCustomerDto);
	Task<CustomerDto?> DeleteCustomerAsync(int id);
}
