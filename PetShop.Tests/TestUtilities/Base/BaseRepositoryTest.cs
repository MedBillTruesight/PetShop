using Microsoft.EntityFrameworkCore;
using PetShop.Domain;
using PetShop.Infrastructure;

/// <summary>
/// Base test class for repository tests providing DbContext setup and cleanup.
/// Creates a new in-memory database for each test.
/// </summary>
public abstract class BaseRepositoryTest : BaseTest, IDisposable
{
    protected readonly PetShopDbContext DbContext;

    protected BaseRepositoryTest()
    {
        var options = new DbContextOptionsBuilder<PetShopDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        DbContext = new PetShopDbContext(options);
    }

    /// <summary>
    /// Creates a test customer and saves it to the database.
    /// </summary>
    protected async Task<Customer> CreateAndSaveCustomerAsync(
        string firstName = "Test",
        string lastName = "Customer",
        string? email = null,
        string? phone = null)
    {
        var customer = new Customer(firstName, lastName, email, phone);
        DbContext.Customers.Add(customer);
        await DbContext.SaveChangesAsync();
        return customer;
    }

    /// <summary>
    /// Creates a test order with optional pets and saves it to the database.
    /// </summary>
    protected async Task<Order> CreateAndSaveOrderAsync(
        Guid customerId,
        DateOnly pickupDate,
        params Pet[] pets)
    {
        var order = new Order(customerId, pickupDate);

        foreach (var pet in pets)
        {
            // Update pet with correct order ID
            var petWithOrderId = new Pet(order.Id, pet.Name, pet.Price, pet.Kind, pet.Color);
            order.AddPet(petWithOrderId);
        }

        DbContext.Orders.Add(order);
        await DbContext.SaveChangesAsync();
        return order;
    }

    /// <summary>
    /// Creates a test pet (not saved to database).
    /// </summary>
    protected Pet CreateTestPet(Guid orderId, string name = "TestPet", decimal price = 100m)
    {
        return new Pet(orderId, name, price);
    }

    /// <summary>
    /// Clears all data from the database.
    /// </summary>
    protected async Task ClearDatabaseAsync()
    {
        DbContext.Orders.RemoveRange(DbContext.Orders);
        DbContext.Customers.RemoveRange(DbContext.Customers);
        await DbContext.SaveChangesAsync();
    }

    protected override void TearDown()
    {
        DbContext.Dispose();
        base.TearDown();
    }
}