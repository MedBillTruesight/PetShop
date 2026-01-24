using PetShop.Domain;

/// <summary>
/// Factory for generating consistent test data across different test scenarios.
/// Provides methods to create related entities with proper relationships.
/// </summary>
public static class TestDataFactory
{
    private static int _sequenceCounter = 0;

    /// <summary>
    /// Creates a customer with a unique name to avoid conflicts.
    /// </summary>
    public static Customer CreateCustomer(
        string? firstName = null,
        string? lastName = null,
        string? email = null,
        string? phone = null)
    {
        var sequence = Interlocked.Increment(ref _sequenceCounter);
        return new Customer(
            firstName ?? $"TestFirst{sequence}",
            lastName ?? $"TestLast{sequence}",
            email ?? $"test{sequence}@example.com",
            phone ?? $"555-100{sequence % 10}");
    }

    /// <summary>
    /// Creates an order with a customer and optional pets.
    /// </summary>
    public static Order CreateOrderWithCustomer(
        int daysFromToday = 1,
        int petCount = 0,
        decimal petPrice = 100m)
    {
        var customer = CreateCustomer();
        var order = new Order(customer.Id, Today.AddDays(daysFromToday));

        for (int i = 0; i < petCount; i++)
        {
            var pet = CreatePet(order.Id, $"Pet{i + 1}", petPrice + (i * 10));
            order.AddPet(pet);
        }

        return order;
    }

    /// <summary>
    /// Creates a pet with specified parameters.
    /// </summary>
    public static Pet CreatePet(
        Guid orderId,
        string? name = null,
        decimal price = 100m,
        string? kind = null,
        string? color = null)
    {
        var sequence = Interlocked.Increment(ref _sequenceCounter);
        return new Pet(
            orderId,
            name ?? $"TestPet{sequence}",
            price,
            kind,
            color);
    }

    /// <summary>
    /// Creates a complete order lifecycle: Open -> Processing -> Delivered
    /// </summary>
    public static Order CreateDeliveredOrder()
    {
        var order = CreateOrderWithCustomer(petCount: 2, petPrice: 150m);
        order.TransitionToProcessing();
        order.TransitionToDelivered();
        return order;
    }

    /// <summary>
    /// Creates multiple customers for bulk operations testing.
    /// </summary>
    public static IEnumerable<Customer> CreateCustomers(int count)
    {
        for (int i = 0; i < count; i++)
        {
            yield return CreateCustomer();
        }
    }

    /// <summary>
    /// Creates multiple orders for the same customer.
    /// </summary>
    public static IEnumerable<Order> CreateOrdersForCustomer(Guid customerId, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var order = new Order(customerId, Today.AddDays(i + 1));
            var pet = CreatePet(order.Id, $"Pet{i + 1}", 100m + (i * 25));
            order.AddPet(pet);
            yield return order;
        }
    }

    /// <summary>
    /// Resets the sequence counter (useful for deterministic tests).
    /// </summary>
    public static void ResetSequence()
    {
        Interlocked.Exchange(ref _sequenceCounter, 0);
    }

    private static DateOnly Today => DateOnly.FromDateTime(DateTime.Today);
}