using PetShop.Domain.Common;

namespace PetShop.Domain.Entities;
public class OrderPet : BaseEntity
{
    public Guid PetId { get; set; }
    public Guid OrderId { get; set; }    
    public virtual Order Order { get; set; } = null!;
    public virtual Pet Pet { get; set; } = null!;
}