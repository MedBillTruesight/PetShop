using System.ComponentModel.DataAnnotations;

namespace PetShop.Api.Dtos
{
    public class CreateCustomerDto
    {
        public required string FirstName { get; set; } = null!;
        public required string LastName { get; set; } = null!;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }
}
