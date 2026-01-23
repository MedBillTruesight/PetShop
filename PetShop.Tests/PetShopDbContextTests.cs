using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PetShop.Domain;
using PetShop.Infrastructure;

namespace PetShop.Tests;

/// <summary>
/// Integration tests for the PetShopDbContext.
/// Tests verify entity configurations, relationships, and database operations using InMemory provider.
/// </summary>
public class PetShopDbContextTests
{
    private static PetShopDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PetShopDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new PetShopDbContext(options);
    }

    [Fact]
    public void DbContext_CanBeCreated_ShouldSucceed()
    {
        // Act
        using var context = CreateDbContext();

        // Assert
        context.Should().NotBeNull();
        context.Customers.Should().NotBeNull();
        context.Orders.Should().NotBeNull();
        context.Pets.Should().NotBeNull();
    }

    [Fact]
    public async Task DbContext_CanAddCustomer_ShouldSucceed()
    {
        // Arrange
        using var context = CreateDbContext();
        var customer = new Customer("John", "Doe", "john@example.com", "555-1234");

        // Act
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        // Assert
        var savedCustomer = await context.Customers.FindAsync(customer.Id);
        savedCustomer.Should().NotBeNull();
        savedCustomer!.FirstName.Should().Be("John");
        savedCustomer.LastName.Should().Be("Doe");
        savedCustomer.Email.Should().Be("john@example.com");
        savedCustomer.Phone.Should().Be("555-1234");
    }

    [Fact]
    public async Task DbContext_CanAddOrder_ShouldSucceed()
    {
        // Arrange
        using var context = CreateDbContext();
        var customer = new Customer("John", "Doe");
        var order = new Order(customer.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));

        // Act
        context.Customers.Add(customer);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        // Assert
        var savedOrder = await context.Orders.FindAsync(order.Id);
        savedOrder.Should().NotBeNull();
        savedOrder!.CustomerId.Should().Be(customer.Id);
        savedOrder.Status.Should().Be(OrderStatus.Open);
        savedOrder.PickupDate.Should().Be(DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
    }

    [Fact]
    public async Task DbContext_CanAddPet_ShouldSucceed()
    {
        // Arrange
        using var context = CreateDbContext();
        var customer = new Customer("John", "Doe");
        var order = new Order(customer.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var pet = new Pet(order.Id, "Fluffy", 100m, "Dog", "Brown");

        // Act
        context.Customers.Add(customer);
        context.Orders.Add(order);
        context.Pets.Add(pet);
        await context.SaveChangesAsync();

        // Assert
        var savedPet = await context.Pets.FindAsync(pet.Id);
        savedPet.Should().NotBeNull();
        savedPet!.Name.Should().Be("Fluffy");
        savedPet.Price.Should().Be(100m);
        savedPet.Kind.Should().Be("Dog");
        savedPet.Color.Should().Be("Brown");
        savedPet.OrderId.Should().Be(order.Id);
    }

    [Fact]
    public async Task DbContext_CustomerOrdersRelationship_ShouldBeConfigured()
    {
        // Arrange
        using var context = CreateDbContext();
        var customer = new Customer("John", "Doe");
        var order1 = new Order(customer.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var order2 = new Order(customer.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(2)));

        // Act
        context.Customers.Add(customer);
        context.Orders.Add(order1);
        context.Orders.Add(order2);
        await context.SaveChangesAsync();

        // Assert
        var savedCustomer = await context.Customers
            .Include(c => c.Orders)
            .FirstOrDefaultAsync(c => c.Id == customer.Id);

        savedCustomer.Should().NotBeNull();
        savedCustomer!.Orders.Should().HaveCount(2);
        savedCustomer.Orders.Should().Contain(o => o.Id == order1.Id);
        savedCustomer.Orders.Should().Contain(o => o.Id == order2.Id);
    }

    [Fact]
    public async Task DbContext_OrderPetsRelationship_ShouldBeConfigured()
    {
        // Arrange
        using var context = CreateDbContext();
        var customer = new Customer("John", "Doe");
        var order = new Order(customer.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var pet1 = new Pet(order.Id, "Fluffy", 100m);
        var pet2 = new Pet(order.Id, "Spot", 150m);

        // Act
        context.Customers.Add(customer);
        context.Orders.Add(order);
        context.Pets.Add(pet1);
        context.Pets.Add(pet2);
        await context.SaveChangesAsync();

        // Assert
        var savedOrder = await context.Orders
            .Include(o => o.Pets)
            .FirstOrDefaultAsync(o => o.Id == order.Id);

        savedOrder.Should().NotBeNull();
        savedOrder!.Pets.Should().HaveCount(2);
        savedOrder.Pets.Should().Contain(p => p.Id == pet1.Id);
        savedOrder.Pets.Should().Contain(p => p.Id == pet2.Id);
    }

    [Fact]
    public async Task DbContext_OrderCustomerRelationship_ShouldBeConfigured()
    {
        // Arrange
        using var context = CreateDbContext();
        var customer = new Customer("John", "Doe");
        var order = new Order(customer.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));

        // Act
        context.Customers.Add(customer);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        // Assert
        var savedOrder = await context.Orders
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.Id == order.Id);

        savedOrder.Should().NotBeNull();
        savedOrder!.Customer.Should().NotBeNull();
        savedOrder.Customer.Id.Should().Be(customer.Id);
        savedOrder.Customer.FirstName.Should().Be("John");
    }

    [Fact]
    public async Task DbContext_PetOrderRelationship_ShouldBeConfigured()
    {
        // Arrange
        using var context = CreateDbContext();
        var customer = new Customer("John", "Doe");
        var order = new Order(customer.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var pet = new Pet(order.Id, "Fluffy", 100m);

        // Act
        context.Customers.Add(customer);
        context.Orders.Add(order);
        context.Pets.Add(pet);
        await context.SaveChangesAsync();

        // Assert
        var savedPet = await context.Pets
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.Id == pet.Id);

        savedPet.Should().NotBeNull();
        savedPet!.Order.Should().NotBeNull();
        savedPet.Order.Id.Should().Be(order.Id);
    }

    [Fact]
    public async Task DbContext_OptionalFields_ShouldAllowNullValues()
    {
        // Arrange
        using var context = CreateDbContext();
        var customer = new Customer("John", "Doe"); // No email or phone
        var order = new Order(customer.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var pet = new Pet(order.Id, "Fluffy", 100m); // No kind or color

        // Act
        context.Customers.Add(customer);
        context.Orders.Add(order);
        context.Pets.Add(pet);
        await context.SaveChangesAsync();

        // Assert
        var savedCustomer = await context.Customers.FindAsync(customer.Id);
        savedCustomer.Should().NotBeNull();
        savedCustomer!.Email.Should().BeNull();
        savedCustomer.Phone.Should().BeNull();

        var savedPet = await context.Pets.FindAsync(pet.Id);
        savedPet.Should().NotBeNull();
        savedPet!.Kind.Should().BeNull();
        savedPet.Color.Should().BeNull();

        var savedOrder = await context.Orders.FindAsync(order.Id);
        savedOrder.Should().NotBeNull();
        savedOrder!.ActualCost.Should().BeNull();
    }

    [Fact]
    public async Task DbContext_RequiredFields_ShouldBeEnforced()
    {
        // Arrange
        using var context = CreateDbContext();
        var customer = new Customer("John", "Doe");
        var order = new Order(customer.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));

        // Act
        context.Customers.Add(customer);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        // Assert - Verify required fields are persisted
        var savedCustomer = await context.Customers.FindAsync(customer.Id);
        savedCustomer.Should().NotBeNull();
        savedCustomer!.FirstName.Should().NotBeNullOrEmpty();
        savedCustomer.LastName.Should().NotBeNullOrEmpty();

        var savedOrder = await context.Orders.FindAsync(order.Id);
        savedOrder.Should().NotBeNull();
        savedOrder!.CustomerId.Should().NotBeEmpty();
        savedOrder.Status.Should().Be(OrderStatus.Open);
    }

    [Fact]
    public async Task DbContext_DateOnlyConversion_ShouldWorkCorrectly()
    {
        // Arrange
        using var context = CreateDbContext();
        var customer = new Customer("John", "Doe");
        var pickupDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5));
        var order = new Order(customer.Id, pickupDate);

        // Act
        context.Customers.Add(customer);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        // Assert
        var savedOrder = await context.Orders.FindAsync(order.Id);
        savedOrder.Should().NotBeNull();
        savedOrder!.PickupDate.Should().Be(pickupDate);
    }

    [Fact]
    public async Task DbContext_OrderStatusConversion_ShouldWorkCorrectly()
    {
        // Arrange
        using var context = CreateDbContext();
        var customer = new Customer("John", "Doe");
        var order = new Order(customer.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var pet = new Pet(order.Id, "Fluffy", 100m);
        order.AddPet(pet);
        order.TransitionToProcessing();
        order.TransitionToDelivered();

        // Act
        context.Customers.Add(customer);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        // Assert
        var savedOrder = await context.Orders.FindAsync(order.Id);
        savedOrder.Should().NotBeNull();
        savedOrder!.Status.Should().Be(OrderStatus.Delivered);
        savedOrder.ActualCost.Should().Be(100m);
    }
}
