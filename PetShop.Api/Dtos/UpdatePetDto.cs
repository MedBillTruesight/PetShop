using System.ComponentModel.DataAnnotations;

namespace PetShop.Api.Dtos
{
    public class UpdatePetDto
    {
        public Guid Id {  get; set; }

        [StringLength(10, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 10 characters.")]

        public string Name { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }
        public string Kind { get; set; }
        public string Color { get; set; }
        public string Breed { get; set; }
    }
}
