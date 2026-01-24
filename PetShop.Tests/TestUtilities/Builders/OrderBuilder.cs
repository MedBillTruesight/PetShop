using PetShop.Domain;

/// <summary>
/// Fluent builder for creating Order domain entities in tests.
/// Handles complex order creation including pets and state transitions.
/// </summary>
public class OrderBuilder
{
    private Guid _customerId = Guid.NewGuid();
    private DateOnly _pickupDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
    private readonly List<Pet> _pets = new();
    private OrderStatus _targetStatus = OrderStatus.Open;

    public OrderBuilder WithCustomerId(Guid customerId)
    {
        _customerId = customerId;
        return this;
    }

    public OrderBuilder WithPickupDate(DateOnly pickupDate)
    {
        _pickupDate = pickupDate;
        return this;
    }

    public OrderBuilder WithPickupDateInDays(int daysFromToday)
    {
        _pickupDate = DateOnly.FromDateTime(DateTime.Today.AddDays(daysFromToday));
        return this;
    }

    public OrderBuilder WithPet(Pet pet)
    {
        _pets.Add(pet);
        return this;
    }

    public OrderBuilder WithPet(string name, decimal price, string? kind = null, string? color = null)
    {
        var orderId = Guid.NewGuid(); // Will be set when order is created
        var pet = new Pet(orderId, name, price, kind, color);
        _pets.Add(pet);
        return this;
    }

    public OrderBuilder WithMultiplePets(int count, decimal basePrice = 100m)
    {
        for (int i = 0; i < count; i++)
        {
            var orderId = Guid.NewGuid(); // Will be set when order is created
            var pet = new Pet(orderId, $"Pet{i + 1}", basePrice + (i * 10), "Dog", "Brown");
            _pets.Add(pet);
        }
        return this;
    }

    public OrderBuilder WithTargetStatus(OrderStatus status)
    {
        _targetStatus = status;
        return this;
    }

    public OrderBuilder ReadyForProcessing()
    {
        _targetStatus = OrderStatus.Processing;
        if (_pets.Count == 0)
        {
            WithPet("TestPet", 50m);
        }
        return this;
    }

    public OrderBuilder ReadyForDelivery()
    {
        _targetStatus = OrderStatus.Delivered;
        if (_pets.Count == 0)
        {
            WithPet("TestPet", 50m);
        }
        return this;
    }

    public Order Build()
    {
        var order = new Order(_customerId, _pickupDate);

        // Add pets with correct order ID
        foreach (var petTemplate in _pets)
        {
            var pet = new Pet(order.Id, petTemplate.Name, petTemplate.Price, petTemplate.Kind, petTemplate.Color);
            order.AddPet(pet);
        }

        // Transition to target status
        if (_targetStatus == OrderStatus.Processing && _pets.Count > 0)
        {
            order.TransitionToProcessing();
        }
        else if (_targetStatus == OrderStatus.Delivered && _pets.Count > 0)
        {
            order.TransitionToProcessing();
            order.TransitionToDelivered();
        }

        return order;
    }
}