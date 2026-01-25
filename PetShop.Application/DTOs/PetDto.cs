using PetShop.Domain.Enums;

namespace PetShop.Application.DTOs;

public class PetDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public PetKind Kind { get; set; }
    public string Color { get; set; } = string.Empty;
     public bool IsVaccinated { get; set; }
    public string Breed { get; set; } = string.Empty;
    public int AgeInMonths { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class CreatePetDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public PetKind Kind { get; set; }
    public string Color { get; set; } = string.Empty;
    public string Breed { get; set; } = string.Empty;
     public bool IsVaccinated { get; set; }
    public int AgeInMonths { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class UpdatePetDto
{
    public string? Name { get; set; }
    public decimal? Price { get; set; }
    public PetKind? Kind { get; set; }
    public string? Color { get; set; }
    public bool? IsVaccinated { get; set; }
    public string? Breed { get; set; }
    public int? AgeInMonths { get; set; }
    public string? Description { get; set; }
}