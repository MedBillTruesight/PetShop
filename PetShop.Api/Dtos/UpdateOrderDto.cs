using PetShop.Api.Enums;
using PetShop.Api.Validation;

namespace PetShop.Api.Dtos
{
    public class UpdateOrderDto
    {
        [FutureDate(ErrorMessage = "Pickup date must be today or in the future")]
        public DateTime? PickupDate { get; set; }
        public OrderStatus? Status { get; set; }
        public List<UpdatePetDto>? Pets { get; set; }
    }
}
