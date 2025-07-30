using PetShop.Domain.Enums;

namespace PetShop.Domain.Entities;

public class OrderPet
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public int OrderId { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;        
    
    public virtual Order Order { get; set; } = null!;
    public virtual Pet Pet { get; set; } = null!;
}
