using System.ComponentModel.DataAnnotations;

namespace PetShop.Api.Dtos
{
    public class CreatePetDto
    {
        [Required]
        public string Name { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        public required string Kind { get; set; }
        public string Color { get; set; }
        public string Breed { get; set; }
    }
}
