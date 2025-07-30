using PetShop.Application.DTOs;
using PetShop.Domain.Entities;

namespace PetShop.Application.Mappers;

public interface ICustomerMapper
{
    Customer ToDomain(CreateCustomerDto dto);
    Customer ToDomain(UpdateCustomerDto dto);
    CustomerDto ToDto(Customer customer);
}