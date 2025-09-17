using System.ComponentModel.DataAnnotations;

namespace PetShop.Api.Dtos
{
    public class UpdateCustomerDto
    {
        public required string FirstName { get; set; }

        public required string LastName { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public string Phone { get; set; }
        public string Address { get; set; }
    }
}
