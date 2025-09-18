using System.ComponentModel.DataAnnotations;

namespace PetShop.Api.Dtos
{
    public class UpdateCustomerDto
    {
        [StringLength(10, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 10 characters.")]
        public string FirstName { get; set; }
        [StringLength(10, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 10 characters.")]
        public string LastName { get; set; }
        [EmailAddress]
        public string Email { get; set; }

        public string? Phone { get; set; }
        public string? Address { get; set; }
    }
}
