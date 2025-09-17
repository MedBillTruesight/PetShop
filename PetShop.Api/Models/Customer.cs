using System.ComponentModel.DataAnnotations;

namespace PetShop.Api.Models
{
    public class Customer
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? Email { get; set; }
        public string? Phone { get; set; }

        public string? Address { get; set; }

        public List<Order> Orders { get; set; } = new();
    }
}
