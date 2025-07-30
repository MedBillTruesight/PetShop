using PetShop.Domain.Enums;

namespace PetShop.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DateTime PickupDate { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Open;
    public decimal? ActualCost { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
        
    public virtual Customer Customer { get; set; } = null!;
    public virtual ICollection<OrderPet> OrderPets { get; set; } = new List<OrderPet>();
}
