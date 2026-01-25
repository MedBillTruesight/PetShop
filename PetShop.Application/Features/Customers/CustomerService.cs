using PetShop.Application.DTOs;
using PetShop.Application.Interfaces.Mappers;
using PetShop.Application.Interfaces.Services;
using PetShop.Domain.Entities;
using PetShop.Domain.Exceptions;
using PetShop.Domain.Interfaces.Repositories;

namespace PetShop.Application.Features.Customers;
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
        var isEmailUnique = await ValidateUniqueEmail(createCustomerDto.Email);
        if (!isEmailUnique)
            throw new AppException($"A customer with the email '{createCustomerDto.Email}' already exists.");

        Customer customer = _customerMapper.ToDomain(createCustomerDto);

        await _customerRepository.CreateCustomerAsync(customer);

        return _customerMapper.ToDto(customer);
    }

    public async Task<CustomerDto?> GetCustomerAsync(Guid id)
    {
        var customer = await _customerRepository.GetCustomerByIdAsync(id);

        if (customer == null)
            throw new AppException($"Customer with id '{id}' not found.");
            
        return _customerMapper.ToDto(customer);
    }

    public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
    {
        var customers = await _customerRepository.GetAllCustomersAsync();

        return customers.Select(_customerMapper.ToDto);
    }



    public async Task<CustomerDto?> UpdateCustomerAsync(Guid id, UpdateCustomerDto updateCustomerDto)
    {
        var customer = _customerMapper.ToDomain(updateCustomerDto);
        customer.Id = id;

        var updatedCustomer = await _customerRepository.UpdateCustomerAsync(customer);
        if (updatedCustomer == null)
            throw new AppException($"Customer with id '{id}' not found.");

        return _customerMapper.ToDto(updatedCustomer!);
    }

    public async Task<CustomerDto?> DeleteCustomerAsync(Guid id)
    {
        var deletedCustomer = await _customerRepository.DeleteCustomerByIdAsync(id);
        if (deletedCustomer == null)  
            throw new AppException($"Customer with id '{id}' not found.");

        return _customerMapper.ToDto(deletedCustomer);
    }

    private async Task<bool> ValidateUniqueEmail(string email)
    {
        var existingCustomers = await _customerRepository.GetAllCustomersAsync();
        return !existingCustomers.Any(p => p.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}