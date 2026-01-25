using PetShop.Domain.Common;
using PetShop.Domain.Enums;

namespace PetShop.Domain.Entities;
public class Pet : BaseEntity
{
    public string? Name { get; set; }
    public decimal Price { get; set; }
    public PetKind Kind { get; set; }
    public int AgeInMonths { get; set; }
    public string Color { get; set; } = string.Empty;
    public string Breed { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsVaccinated { get; set;  }

    public virtual ICollection<OrderPet> OrderPets { get; set; } = new List<OrderPet>();
}
