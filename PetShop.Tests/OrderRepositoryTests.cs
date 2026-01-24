using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PetShop.Domain;
using PetShop.Infrastructure.Repositories;

namespace PetShop.Tests;

/// <summary>
/// Integration tests for the OrderRepository.
/// Tests use real DbContext with InMemory provider to verify actual database operations.
/// </summary>
public class OrderRepositoryTests : BaseRepositoryTest
{
    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithExistingOrder_ShouldReturnOrderWithIncludes()
    {
        // Arrange
        var customer = await CreateAndSaveCustomerAsync("John", "Doe");
        var order = await CreateAndSaveOrderAsync(
            customer.Id,
            Tomorrow,
            CreateTestPet(customer.Id, "Fluffy", 100m),
            CreateTestPet(customer.Id, "Buddy", 150m));

        var repository = new OrderRepository(DbContext);

        // Act
        var result = await repository.GetByIdAsync(order.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(order.Id);
        result.CustomerId.Should().Be(customer.Id);
        result.Customer.Should().NotBeNull();
        result.Customer!.FirstName.Should().Be("John");
        result.Customer.LastName.Should().Be("Doe");
        result.Pets.Should().HaveCount(2);
        result.Pets.Should().Contain(p => p.Name == "Fluffy" && p.Price == 100m);
        result.Pets.Should().Contain(p => p.Name == "Buddy" && p.Price == 150m);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentOrder_ShouldReturnNull()
    {
        // Arrange
        var repository = new OrderRepository(DbContext);
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithOrderWithoutPets_ShouldReturnOrder()
    {
        // Arrange
        var customer = await CreateAndSaveCustomerAsync("Jane", "Smith");
        var order = await CreateAndSaveOrderAsync(customer.Id, Tomorrow); // No pets

        var repository = new OrderRepository(DbContext);

        // Act
        var result = await repository.GetByIdAsync(order.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(order.Id);
        result.CustomerId.Should().Be(customer.Id);
        result.Pets.Should().NotBeNull().And.BeEmpty();
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WithMultipleOrders_ShouldReturnAllOrders()
    {
        // Arrange
        var customer1 = await CreateAndSaveCustomerAsync("John", "Doe");
        var customer2 = await CreateAndSaveCustomerAsync("Jane", "Smith");

        var order1 = await CreateAndSaveOrderAsync(customer1.Id, Tomorrow,
            CreateTestPet(customer1.Id, "Fluffy", 100m));
        var order2 = await CreateAndSaveOrderAsync(customer2.Id, Tomorrow.AddDays(1),
            CreateTestPet(customer2.Id, "Buddy", 150m));
        var order3 = await CreateAndSaveOrderAsync(customer1.Id, Tomorrow.AddDays(2)); // No pets

        var repository = new OrderRepository(DbContext);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(o => o.Id == order1.Id);
        result.Should().Contain(o => o.Id == order2.Id);
        result.Should().Contain(o => o.Id == order3.Id);

        // Verify includes work
        result.All(o => o.Customer != null).Should().BeTrue();
        result.All(o => o.Pets != null).Should().BeTrue();
    }

    [Fact]
    public async Task GetAllAsync_WithNoOrders_ShouldReturnEmptyCollection()
    {
        // Arrange
        var repository = new OrderRepository(DbContext);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.Should().NotBeNull().And.BeEmpty();
    }

    #endregion

    #region GetByCustomerIdAsync Tests

    [Fact]
    public async Task GetByCustomerIdAsync_WithExistingCustomer_ShouldReturnCustomerOrders()
    {
        // Arrange
        var customer1 = await CreateAndSaveCustomerAsync("John", "Doe");
        var customer2 = await CreateAndSaveCustomerAsync("Jane", "Smith");

        var order1 = await CreateAndSaveOrderAsync(customer1.Id, Tomorrow,
            CreateTestPet(customer1.Id, "Fluffy", 100m));
        var order2 = await CreateAndSaveOrderAsync(customer1.Id, Tomorrow.AddDays(1),
            CreateTestPet(customer1.Id, "Buddy", 150m));
        var order3 = await CreateAndSaveOrderAsync(customer2.Id, Tomorrow); // Different customer

        var repository = new OrderRepository(DbContext);

        // Act
        var result = await repository.GetByCustomerIdAsync(customer1.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(o => o.Id == order1.Id);
        result.Should().Contain(o => o.Id == order2.Id);
        result.Should().NotContain(o => o.Id == order3.Id);

        // Verify all orders belong to the correct customer
        result.All(o => o.CustomerId == customer1.Id).Should().BeTrue();
        result.All(o => o.Customer != null).Should().BeTrue();
    }

    [Fact]
    public async Task GetByCustomerIdAsync_WithCustomerHavingNoOrders_ShouldReturnEmptyCollection()
    {
        // Arrange
        var customer = await CreateAndSaveCustomerAsync("John", "Doe");
        var repository = new OrderRepository(DbContext);

        // Act
        var result = await repository.GetByCustomerIdAsync(customer.Id);

        // Assert
        result.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task GetByCustomerIdAsync_WithNonExistentCustomer_ShouldReturnEmptyCollection()
    {
        // Arrange
        var nonExistentCustomerId = Guid.NewGuid();
        var repository = new OrderRepository(DbContext);

        // Act
        var result = await repository.GetByCustomerIdAsync(nonExistentCustomerId);

        // Assert
        result.Should().NotBeNull().And.BeEmpty();
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidOrder_ShouldCreateOrder()
    {
        // Arrange
        var customer = await CreateAndSaveCustomerAsync("John", "Doe");
        var order = new Order(customer.Id, Tomorrow);
        var pet1 = new Pet(order.Id, "Fluffy", 100m, "Cat", "White");
        var pet2 = new Pet(order.Id, "Buddy", 150m, "Dog", "Brown");
        order.AddPet(pet1);
        order.AddPet(pet2);

        var repository = new OrderRepository(DbContext);

        // Act
        var result = await repository.CreateAsync(order);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.CustomerId.Should().Be(customer.Id);
        result.PickupDate.Should().Be(Tomorrow);
        result.Pets.Should().HaveCount(2);

        // Verify persisted in database
        var persistedOrder = await DbContext.Orders
            .Include(o => o.Customer)
            .Include(o => o.Pets)
            .FirstOrDefaultAsync(o => o.Id == result.Id);

        persistedOrder.Should().NotBeNull();
        persistedOrder!.Customer.Should().NotBeNull();
        persistedOrder.Pets.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateAsync_WithOrderWithoutPets_ShouldCreateOrder()
    {
        // Arrange
        var customer = await CreateAndSaveCustomerAsync("Jane", "Smith");
        var order = new Order(customer.Id, Tomorrow.AddDays(1));

        var repository = new OrderRepository(DbContext);

        // Act
        var result = await repository.CreateAsync(order);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Pets.Should().NotBeNull().And.BeEmpty();

        // Verify persisted
        var persistedOrder = await DbContext.Orders.FindAsync(result.Id);
        persistedOrder.Should().NotBeNull();
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithModifiedPickupDate_ShouldUpdateOrder()
    {
        // Arrange
        var customer = await CreateAndSaveCustomerAsync("John", "Doe");
        var order = await CreateAndSaveOrderAsync(customer.Id, Tomorrow,
            CreateTestPet(customer.Id, "Fluffy", 100m));

        var newPickupDate = Tomorrow.AddDays(5);
        order.UpdatePickupDate(newPickupDate);

        var repository = new OrderRepository(DbContext);

        // Act
        var result = await repository.UpdateAsync(order);

        // Assert
        result.PickupDate.Should().Be(newPickupDate);

        // Verify persisted
        var persistedOrder = await DbContext.Orders.FindAsync(order.Id);
        persistedOrder.Should().NotBeNull();
        persistedOrder!.PickupDate.Should().Be(newPickupDate);
    }

    [Fact]
    public async Task UpdateAsync_WithAddedPets_ShouldUpdateOrder()
    {
        // Arrange
        var customer = await CreateAndSaveCustomerAsync("John", "Doe");
        var order = await CreateAndSaveOrderAsync(customer.Id, Tomorrow); // No pets initially

        var pet1 = new Pet(order.Id, "Fluffy", 100m, "Cat", "White");
        var pet2 = new Pet(order.Id, "Buddy", 150m, "Dog", "Brown");
        order.AddPet(pet1);
        order.AddPet(pet2);

        var repository = new OrderRepository(DbContext);

        // Act
        var result = await repository.UpdateAsync(order);

        // Assert
        result.Pets.Should().HaveCount(2);
        result.Pets.Should().Contain(p => p.Name == "Fluffy");
        result.Pets.Should().Contain(p => p.Name == "Buddy");

        // Verify persisted
        var persistedOrder = await DbContext.Orders
            .Include(o => o.Pets)
            .FirstOrDefaultAsync(o => o.Id == order.Id);

        persistedOrder.Should().NotBeNull();
        persistedOrder!.Pets.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateAsync_WithTrackedEntity_ShouldUpdateSuccessfully()
    {
        // Arrange
        var customer = await CreateAndSaveCustomerAsync("John", "Doe");
        var order = await CreateAndSaveOrderAsync(customer.Id, Tomorrow,
            CreateTestPet(customer.Id, "Fluffy", 100m));

        // Modify the tracked entity
        var newPet = new Pet(order.Id, "NewPet", 200m);
        order.AddPet(newPet);

        var repository = new OrderRepository(DbContext);

        // Act
        var result = await repository.UpdateAsync(order);

        // Assert
        result.Pets.Should().HaveCount(2);
        result.Pets.Should().Contain(p => p.Name == "Fluffy");
        result.Pets.Should().Contain(p => p.Name == "NewPet");

        // Verify persisted
        var persistedOrder = await DbContext.Orders
            .Include(o => o.Pets)
            .FirstOrDefaultAsync(o => o.Id == order.Id);

        persistedOrder.Should().NotBeNull();
        persistedOrder!.Pets.Should().HaveCount(2);
        persistedOrder.Pets.Should().Contain(p => p.Name == "Fluffy");
        persistedOrder.Pets.Should().Contain(p => p.Name == "NewPet");
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithExistingOrder_ShouldDeleteOrder()
    {
        // Arrange
        var customer = await CreateAndSaveCustomerAsync("John", "Doe");
        var order = await CreateAndSaveOrderAsync(customer.Id, Tomorrow,
            CreateTestPet(customer.Id, "Fluffy", 100m));

        var repository = new OrderRepository(DbContext);

        // Act
        await repository.DeleteAsync(order.Id);

        // Assert
        var deletedOrder = await DbContext.Orders.FindAsync(order.Id);
        deletedOrder.Should().BeNull();

        // Pets should also be deleted (cascade delete)
        var petsCount = await DbContext.Pets.CountAsync(p => p.OrderId == order.Id);
        petsCount.Should().Be(0);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentOrder_ShouldNotThrow()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var repository = new OrderRepository(DbContext);

        // Act & Assert
        await repository.DeleteAsync(nonExistentId); // Should not throw

        // Verify no changes
        var orderCount = await DbContext.Orders.CountAsync();
        orderCount.Should().Be(0);
    }

    #endregion
}