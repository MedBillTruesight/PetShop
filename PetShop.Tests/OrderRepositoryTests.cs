using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PetShop.Domain;
using PetShop.Infrastructure;
using PetShop.Infrastructure.Repositories;

namespace PetShop.Tests;

/// <summary>
/// Integration tests for the OrderRepository.
/// Tests use real DbContext with InMemory provider to verify actual database operations.
/// </summary>
public class OrderRepositoryTests
{
    private static PetShopDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PetShopDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new PetShopDbContext(options);
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithExistingOrder_ShouldReturnOrder()
    {
        // Arrange
        using var context = CreateDbContext();
        var repository = new OrderRepository(context);
        var customer = new Customer("John", "Doe");
        var order = new Order(customer.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));

        context.Customers.Add(customer);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(order.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(order.Id);
        result.CustomerId.Should().Be(customer.Id);
        result.Status.Should().Be(OrderStatus.Open);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentOrder_ShouldReturnNull()
    {
        // Arrange
        using var context = CreateDbContext();
        var repository = new OrderRepository(context);
        var orderId = Guid.NewGuid();

        // Act
        var result = await repository.GetByIdAsync(orderId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithOrderWithPets_ShouldIncludePets()
    {
        // Arrange
        using var context = CreateDbContext();
        var repository = new OrderRepository(context);
        var customer = new Customer("John", "Doe");
        var order = new Order(customer.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var pet1 = new Pet(order.Id, "Fluffy", 100m);
        var pet2 = new Pet(order.Id, "Spot", 150m);
        order.AddPet(pet1);
        order.AddPet(pet2);

        context.Customers.Add(customer);
        context.Orders.Add(order);
        context.Pets.Add(pet1);
        context.Pets.Add(pet2);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(order.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Pets.Should().HaveCount(2);
        result.Pets.Should().Contain(p => p.Id == pet1.Id);
        result.Pets.Should().Contain(p => p.Id == pet2.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithOrder_ShouldIncludeCustomer()
    {
        // Arrange
        using var context = CreateDbContext();
        var repository = new OrderRepository(context);
        var customer = new Customer("John", "Doe");
        var order = new Order(customer.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));

        context.Customers.Add(customer);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(order.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Customer.Should().NotBeNull();
        result.Customer.Id.Should().Be(customer.Id);
        result.Customer.FirstName.Should().Be("John");
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WithNoOrders_ShouldReturnEmptyCollection()
    {
        // Arrange
        using var context = CreateDbContext();
        var repository = new OrderRepository(context);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleOrders_ShouldReturnAllOrders()
    {
        // Arrange
        using var context = CreateDbContext();
        var repository = new OrderRepository(context);
        var customer = new Customer("John", "Doe");
        var order1 = new Order(customer.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var order2 = new Order(customer.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(2)));
        var order3 = new Order(customer.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(3)));

        context.Customers.Add(customer);
        context.Orders.Add(order1);
        context.Orders.Add(order2);
        context.Orders.Add(order3);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(o => o.Id == order1.Id);
        result.Should().Contain(o => o.Id == order2.Id);
        result.Should().Contain(o => o.Id == order3.Id);
    }

    #endregion

    #region GetByCustomerIdAsync Tests

    [Fact]
    public async Task GetByCustomerIdAsync_WithCustomerWithOrders_ShouldReturnCustomerOrders()
    {
        // Arrange
        using var context = CreateDbContext();
        var repository = new OrderRepository(context);
        var customer1 = new Customer("John", "Doe");
        var customer2 = new Customer("Jane", "Smith");
        var order1 = new Order(customer1.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var order2 = new Order(customer1.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(2)));
        var order3 = new Order(customer2.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(3)));

        context.Customers.Add(customer1);
        context.Customers.Add(customer2);
        context.Orders.Add(order1);
        context.Orders.Add(order2);
        context.Orders.Add(order3);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByCustomerIdAsync(customer1.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(o => o.Id == order1.Id);
        result.Should().Contain(o => o.Id == order2.Id);
        result.Should().NotContain(o => o.Id == order3.Id);
    }

    [Fact]
    public async Task GetByCustomerIdAsync_WithCustomerWithNoOrders_ShouldReturnEmptyCollection()
    {
        // Arrange
        using var context = CreateDbContext();
        var repository = new OrderRepository(context);
        var customer = new Customer("John", "Doe");
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByCustomerIdAsync(customer.Id);

        // Assert
        result.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task GetByCustomerIdAsync_WithOrdersWithPets_ShouldIncludePets()
    {
        // Arrange
        using var context = CreateDbContext();
        var repository = new OrderRepository(context);
        var customer = new Customer("John", "Doe");
        var order = new Order(customer.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var pet = new Pet(order.Id, "Fluffy", 100m);
        order.AddPet(pet);

        context.Customers.Add(customer);
        context.Orders.Add(order);
        context.Pets.Add(pet);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByCustomerIdAsync(customer.Id);

        // Assert
        result.Should().HaveCount(1);
        result.First().Pets.Should().HaveCount(1);
        result.First().Pets.First().Id.Should().Be(pet.Id);
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidOrder_ShouldPersistOrder()
    {
        // Arrange
        using var context = CreateDbContext();
        var repository = new OrderRepository(context);
        var customer = new Customer("John", "Doe");
        var order = new Order(customer.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));

        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.CreateAsync(order);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(order.Id);

        var savedOrder = await context.Orders.FindAsync(order.Id);
        savedOrder.Should().NotBeNull();
        savedOrder!.CustomerId.Should().Be(customer.Id);
        savedOrder.Status.Should().Be(OrderStatus.Open);
    }

    [Fact]
    public async Task CreateAsync_WithOrderWithPets_ShouldPersistOrderAndPets()
    {
        // Arrange
        using var context = CreateDbContext();
        var repository = new OrderRepository(context);
        var customer = new Customer("John", "Doe");
        var order = new Order(customer.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var pet = new Pet(order.Id, "Fluffy", 100m);
        order.AddPet(pet);

        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.CreateAsync(order);

        // Assert
        result.Should().NotBeNull();
        var savedOrder = await context.Orders
            .Include(o => o.Pets)
            .FirstOrDefaultAsync(o => o.Id == order.Id);
        savedOrder.Should().NotBeNull();
        savedOrder!.Pets.Should().HaveCount(1);
        savedOrder.Pets.First().Id.Should().Be(pet.Id);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithModifiedOrder_ShouldUpdateOrder()
    {
        // Arrange
        using var context = CreateDbContext();
        var repository = new OrderRepository(context);
        var customer = new Customer("John", "Doe");
        var order = new Order(customer.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));

        context.Customers.Add(customer);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        // Modify order
        var newPickupDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5));
        order.UpdatePickupDate(newPickupDate);

        // Act
        var result = await repository.UpdateAsync(order);

        // Assert
        result.Should().NotBeNull();
        var savedOrder = await context.Orders.FindAsync(order.Id);
        savedOrder.Should().NotBeNull();
        savedOrder!.PickupDate.Should().Be(newPickupDate);
    }

    [Fact]
    public async Task UpdateAsync_WithOrderStateTransition_ShouldPersistStateChange()
    {
        // Arrange
        using var context = CreateDbContext();
        var repository = new OrderRepository(context);
        var customer = new Customer("John", "Doe");
        var order = new Order(customer.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var pet = new Pet(order.Id, "Fluffy", 100m);
        order.AddPet(pet);
        order.TransitionToProcessing();
        order.TransitionToDelivered();

        context.Customers.Add(customer);
        context.Orders.Add(order);
        context.Pets.Add(pet);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.UpdateAsync(order);

        // Assert
        result.Should().NotBeNull();
        var savedOrder = await context.Orders.FindAsync(order.Id);
        savedOrder.Should().NotBeNull();
        savedOrder!.Status.Should().Be(OrderStatus.Delivered);
        savedOrder.ActualCost.Should().Be(100m);
    }

    [Fact]
    public async Task UpdateAsync_WithOrderWithPets_ShouldPreservePets()
    {
        // Arrange
        using var context = CreateDbContext();
        var repository = new OrderRepository(context);
        var customer = new Customer("John", "Doe");
        var order = new Order(customer.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var pet = new Pet(order.Id, "Fluffy", 100m);
        order.AddPet(pet);

        context.Customers.Add(customer);
        context.Orders.Add(order);
        context.Pets.Add(pet);
        await context.SaveChangesAsync();

        // Modify order
        order.UpdatePickupDate(DateOnly.FromDateTime(DateTime.Today.AddDays(2)));

        // Act
        await repository.UpdateAsync(order);

        // Assert
        var savedOrder = await context.Orders
            .Include(o => o.Pets)
            .FirstOrDefaultAsync(o => o.Id == order.Id);
        savedOrder.Should().NotBeNull();
        savedOrder!.Pets.Should().HaveCount(1);
        savedOrder.Pets.First().Id.Should().Be(pet.Id);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithExistingOrder_ShouldRemoveOrder()
    {
        // Arrange
        using var context = CreateDbContext();
        var repository = new OrderRepository(context);
        var customer = new Customer("John", "Doe");
        var order = new Order(customer.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));

        context.Customers.Add(customer);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        // Act
        await repository.DeleteAsync(order.Id);

        // Assert
        var deletedOrder = await context.Orders.FindAsync(order.Id);
        deletedOrder.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentOrder_ShouldNotThrow()
    {
        // Arrange
        using var context = CreateDbContext();
        var repository = new OrderRepository(context);
        var orderId = Guid.NewGuid();

        // Act
        var act = async () => await repository.DeleteAsync(orderId);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DeleteAsync_WithOrderWithPets_ShouldRemoveOrderAndPets()
    {
        // Arrange
        using var context = CreateDbContext();
        var repository = new OrderRepository(context);
        var customer = new Customer("John", "Doe");
        var order = new Order(customer.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var pet = new Pet(order.Id, "Fluffy", 100m);
        order.AddPet(pet);

        context.Customers.Add(customer);
        context.Orders.Add(order);
        context.Pets.Add(pet);
        await context.SaveChangesAsync();

        // Act
        await repository.DeleteAsync(order.Id);

        // Assert
        var deletedOrder = await context.Orders.FindAsync(order.Id);
        deletedOrder.Should().BeNull();

        var deletedPet = await context.Pets.FindAsync(pet.Id);
        deletedPet.Should().BeNull(); // Cascade delete should remove pets
    }

    #endregion
}
