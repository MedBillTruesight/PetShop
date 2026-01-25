using PetShop.Domain.Common;
using PetShop.Domain.Enums;

namespace PetShop.Domain.Entities;
public class Order : BaseEntity
{
   public Guid CustomerId { get; set; } 
   public DateTime PickupDate { get; set; } 
   public OrderStatus Status { get; set; } = OrderStatus.Open; 
   public decimal ActualCost { get; set; }

   public virtual Customer Customer { get; set; } = null!;
   public virtual ICollection<OrderPet> OrderPets { get; set; } = new List<OrderPet>();
}