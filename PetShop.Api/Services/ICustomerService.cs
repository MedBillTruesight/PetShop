using PetShop.Api.Dtos;

namespace PetShop.Api.Services
{
    public interface ICustomerService
    {
        Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto request);
        Task<CustomerDto> GetCustomerAsync(Guid id);
        Task<CustomerDto> UpdateCustomerAsync(Guid id, UpdateCustomerDto request);
    }
}
