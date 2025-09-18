using PetShop.Api.Dtos;

namespace PetShop.Api.Mapper
{
    public static class CustomerMappings
    {
        public static CustomerDto ToDto(this Models.Customer customer)
        {
            return new CustomerDto
            {
                Id = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                Phone = customer.Phone,
            };
        }

        public static Models.Customer ToModel(this CreateCustomerDto createCustomerDto)
        {
            return new Models.Customer
            {
                FirstName = createCustomerDto.FirstName,
                LastName = createCustomerDto.LastName,
                Email = createCustomerDto.Email,
                Phone = createCustomerDto.Phone
            };
        }

    }
}
