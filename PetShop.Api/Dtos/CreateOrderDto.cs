using PetShop.Api.Validation;
using System.ComponentModel.DataAnnotations;

namespace PetShop.Api.Dtos
{
    public class CreateOrderDto
    {
        [Required(ErrorMessage = "Customer Id is Required")]
        public Guid CustomerId { get; set; }
        [Required]
        [FutureDate(ErrorMessage = "Pickup date must be today or in the future")]
        public DateTime PickupDate { get; set; }

        public List<CreatePetDto>? Pets { get; set; } 
    }
}
