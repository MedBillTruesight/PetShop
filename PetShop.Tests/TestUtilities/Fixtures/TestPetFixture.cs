using PetShop.Domain;

/// <summary>
/// Test fixture providing pre-configured Pet entities for common test scenarios.
/// </summary>
public class TestPetFixture
{
    private readonly Guid _orderId = Guid.NewGuid();

    public Pet BasicPet => new PetBuilder()
        .WithOrderId(_orderId)
        .WithMinimalInfo()
        .Build();

    public Pet CompletePet => new PetBuilder()
        .WithOrderId(_orderId)
        .WithCompleteInfo()
        .Build();

    public Pet DogPet => new PetBuilder()
        .WithOrderId(_orderId)
        .AsDog()
        .WithPrice(120m)
        .WithColor("Black")
        .Build();

    public Pet CatPet => new PetBuilder()
        .WithOrderId(_orderId)
        .AsCat()
        .WithPrice(80m)
        .WithColor("Gray")
        .Build();

    public Pet ExpensivePet => new PetBuilder()
        .WithOrderId(_orderId)
        .Expensive(1000m)
        .WithKind("Exotic")
        .WithColor("Rainbow")
        .Build();

    public Pet CheapPet => new PetBuilder()
        .WithOrderId(_orderId)
        .Cheap(10m)
        .WithName("StreetPet")
        .Build();

    public Pet ZeroPricePet => new PetBuilder()
        .WithOrderId(_orderId)
        .WithName("FreePet")
        .WithPrice(0.01m) // Minimum valid price
        .Build();

    public Pet LongNamePet => new PetBuilder()
        .WithOrderId(_orderId)
        .WithName("VeryLongPetNameThatMightCauseDisplayIssuesInUI")
        .WithPrice(50m)
        .Build();

    public Pet SpecialCharsPet => new PetBuilder()
        .WithOrderId(_orderId)
        .WithName("Fluffy & Furrball")
        .WithPrice(125m)
        .WithKind("Cat-Dog Mix")
        .WithColor("Spotted & Striped")
        .Build();
}