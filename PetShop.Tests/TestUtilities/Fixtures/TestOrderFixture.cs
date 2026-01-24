using PetShop.Domain;

/// <summary>
/// Test fixture providing pre-configured Order entities for common test scenarios.
/// </summary>
public class TestOrderFixture
{
    private readonly Guid _customerId = Guid.NewGuid();

    public Order EmptyOrder => new OrderBuilder()
        .WithCustomerId(_customerId)
        .WithPickupDateInDays(1)
        .Build();

    public Order OrderWithOnePet => new OrderBuilder()
        .WithCustomerId(_customerId)
        .WithPickupDateInDays(1)
        .WithPet("Fluffy", 100m, "Cat", "White")
        .Build();

    public Order OrderWithMultiplePets => new OrderBuilder()
        .WithCustomerId(_customerId)
        .WithPickupDateInDays(2)
        .WithPet("Buddy", 150m, "Dog", "Brown")
        .WithPet("Whiskers", 75m, "Cat", "Black")
        .WithPet("Max", 200m, "Dog", "Golden")
        .Build();

    public Order ProcessingOrder => new OrderBuilder()
        .WithCustomerId(_customerId)
        .WithPickupDateInDays(1)
        .ReadyForProcessing()
        .Build();

    public Order DeliveredOrder => new OrderBuilder()
        .WithCustomerId(_customerId)
        .WithPickupDateInDays(3)
        .ReadyForDelivery()
        .Build();

    public Order FuturePickupOrder => new OrderBuilder()
        .WithCustomerId(_customerId)
        .WithPickupDateInDays(30)
        .WithPet("FuturePet", 120m)
        .Build();

    public Order TodayPickupOrder => new OrderBuilder()
        .WithCustomerId(_customerId)
        .WithPickupDateInDays(0)
        .WithPet("TodayPet", 90m)
        .Build();

    public Order ExpensiveOrder => new OrderBuilder()
        .WithCustomerId(_customerId)
        .WithPickupDateInDays(1)
        .WithPet("ExpensivePet1", 500m, "Exotic", "Rare")
        .WithPet("ExpensivePet2", 750m, "Premium", "Special")
        .Build();
}