using PetShop.Application.DTOs;

namespace PetShop.Application.Mappers;

public interface ICustomerMapper
{
    Domain.Entities.Customer ToDomain(CreateCustomerDto dto);
    Domain.Entities.Customer ToDomain(UpdateCustomerDto dto);
    CustomerDto ToDto(Domain.Entities.Customer customer);
}