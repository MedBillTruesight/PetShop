using PetShop.Application.DTOs;
using PetShop.Domain.Entities;
using AutoMapper;

namespace PetShop.Application.Mappers;

public class CustomerMapper: ICustomerMapper
{
    private readonly IMapper _mapper;

    public CustomerMapper(IMapper mapper)
    {
        _mapper = mapper;
    }
    
    public Customer ToDomain(CreateCustomerDto dto)
        => _mapper.Map<Customer>(dto);
    
    public Customer ToDomain(UpdateCustomerDto dto)
        => _mapper.Map<Customer>(dto);

    public CustomerDto ToDto(Customer customer)
        => _mapper.Map<CustomerDto>(customer);
}