using PetShop.Domain.Enums;

namespace PetShop.Domain.Entities;

public class Pet
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public decimal Price { get; set; }
    public PetKind Kind { get; set; }
    public string Color { get; set; } = string.Empty;
    public string Breed { get; set; } = string.Empty;
    public int AgeInMonths { get; set; }
    public string Description { get; set; } = string.Empty;
        
    public virtual ICollection<OrderPet> OrderPets { get; set; } = new List<OrderPet>();
}
