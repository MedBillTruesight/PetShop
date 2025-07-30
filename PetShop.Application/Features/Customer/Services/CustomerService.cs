
using PetShop.Application.DTOs;
using PetShop.Application.Features.Customer.Interfaces;
using PetShop.Application.Mappers;
using PetShop.Domain.Enums;
using PetShop.Domain.Interfaces;

namespace PetShop.Application.Features.Customer.Services;

public class CustomerService : ICustomerService
{
	private readonly ICustomerRepository _customerRepository;
	private readonly ICustomerMapper _customerMapper;

	public CustomerService(ICustomerRepository customerRepository, ICustomerMapper customerMapper)
	{
		_customerRepository = customerRepository;
		_customerMapper = customerMapper;
	}
	
	public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto createCustomerDto)
	{
		Domain.Entities.Customer customer = _customerMapper.ToDomain(createCustomerDto);
		
		_customerRepository.CreateCustomerAsync(customer);

		return _customerMapper.ToDto(customer);
	}

	public async Task<CustomerDto?> GetCustomerAsync(int id)
	{
		var customer = await _customerRepository.GetCustomerByIdAsync(id);
		if (customer == null) return null;
		return _customerMapper.ToDto(customer);
	}

	public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
	{
		var customers = await _customerRepository.GetAllCustomersAsync();

		return customers.Select(_customerMapper.ToDto);
	}

	

	public async Task<CustomerDto?> UpdateCustomerAsync(int id, UpdateCustomerDto updateCustomerDto)
	{
		var customer =  _customerMapper.ToDomain(updateCustomerDto);
		customer.Id = id;
		
		var updatedCustomer = await _customerRepository.UpdateCustomerAsync(customer);
		if (updatedCustomer == null)
		{
			return null;
		}

		return _customerMapper.ToDto(updatedCustomer);
	}

	public async Task<CustomerDto?> DeleteCustomerAsync(int id)
	{
		var deletedCustomer = await _customerRepository.DeleteCustomerByIdAsync(id);
		if (deletedCustomer == null) return null;
		
		return _customerMapper.ToDto(deletedCustomer);
	}
}