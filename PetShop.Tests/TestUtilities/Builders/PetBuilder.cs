using PetShop.Domain;

/// <summary>
/// Fluent builder for creating Pet domain entities in tests.
/// Provides easy creation of pets with various configurations.
/// </summary>
public class PetBuilder
{
    private Guid _orderId = Guid.NewGuid();
    private string _name = "Fluffy";
    private decimal _price = 100m;
    private string? _kind;
    private string? _color;

    public PetBuilder WithOrderId(Guid orderId)
    {
        _orderId = orderId;
        return this;
    }

    public PetBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public PetBuilder WithPrice(decimal price)
    {
        _price = price;
        return this;
    }

    public PetBuilder WithKind(string? kind)
    {
        _kind = kind;
        return this;
    }

    public PetBuilder WithColor(string? color)
    {
        _color = color;
        return this;
    }

    public PetBuilder AsDog()
    {
        _kind = "Dog";
        _name = "Buddy";
        return this;
    }

    public PetBuilder AsCat()
    {
        _kind = "Cat";
        _name = "Whiskers";
        return this;
    }

    public PetBuilder WithCompleteInfo()
    {
        _name = "Max";
        _price = 150m;
        _kind = "Dog";
        _color = "Golden";
        return this;
    }

    public PetBuilder WithMinimalInfo()
    {
        _name = "TestPet";
        _price = 50m;
        _kind = null;
        _color = null;
        return this;
    }

    public PetBuilder Expensive(decimal price = 500m)
    {
        _price = price;
        _name = "PremiumPet";
        return this;
    }

    public PetBuilder Cheap(decimal price = 25m)
    {
        _price = price;
        _name = "BudgetPet";
        return this;
    }

    public Pet Build()
    {
        return new Pet(_orderId, _name, _price, _kind, _color);
    }
}