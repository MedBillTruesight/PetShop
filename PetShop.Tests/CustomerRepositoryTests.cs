using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PetShop.Domain;
using PetShop.Infrastructure;
using PetShop.Infrastructure.Repositories;

namespace PetShop.Tests;

/// <summary>
/// Integration tests for the CustomerRepository.
/// Tests use real DbContext with InMemory provider to verify actual database operations.
/// </summary>
public class CustomerRepositoryTests
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
    public async Task GetByIdAsync_WithExistingCustomer_ShouldReturnCustomer()
    {
        // Arrange
        using var context = CreateDbContext();
        var repository = new CustomerRepository(context);
        var customer = new Customer("John", "Doe", "john@example.com", "555-1234");
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(customer.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(customer.Id);
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Email.Should().Be("john@example.com");
        result.Phone.Should().Be("555-1234");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentCustomer_ShouldReturnNull()
    {
        // Arrange
        using var context = CreateDbContext();
        var repository = new CustomerRepository(context);
        var customerId = Guid.NewGuid();

        // Act
        var result = await repository.GetByIdAsync(customerId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithCustomerWithOrders_ShouldIncludeOrders()
    {
        // Arrange
        using var context = CreateDbContext();
        var repository = new CustomerRepository(context);
        var customer = new Customer("John", "Doe");
        var order1 = new Order(customer.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var order2 = new Order(customer.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(2)));

        context.Customers.Add(customer);
        context.Orders.Add(order1);
        context.Orders.Add(order2);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(customer.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Orders.Should().HaveCount(2);
        result.Orders.Should().Contain(o => o.Id == order1.Id);
        result.Orders.Should().Contain(o => o.Id == order2.Id);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WithNoCustomers_ShouldReturnEmptyCollection()
    {
        // Arrange
        using var context = CreateDbContext();
        var repository = new CustomerRepository(context);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleCustomers_ShouldReturnAllCustomers()
    {
        // Arrange
        using var context = CreateDbContext();
        var repository = new CustomerRepository(context);
        var customer1 = new Customer("John", "Doe");
        var customer2 = new Customer("Jane", "Smith");
        var customer3 = new Customer("Bob", "Johnson");

        context.Customers.Add(customer1);
        context.Customers.Add(customer2);
        context.Customers.Add(customer3);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(c => c.Id == customer1.Id);
        result.Should().Contain(c => c.Id == customer2.Id);
        result.Should().Contain(c => c.Id == customer3.Id);
    }

    [Fact]
    public async Task GetAllAsync_WithCustomersWithOrders_ShouldIncludeOrders()
    {
        // Arrange
        using var context = CreateDbContext();
        var repository = new CustomerRepository(context);
        var customer = new Customer("John", "Doe");
        var order = new Order(customer.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));

        context.Customers.Add(customer);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().Orders.Should().HaveCount(1);
        result.First().Orders.First().Id.Should().Be(order.Id);
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidCustomer_ShouldPersistCustomer()
    {
        // Arrange
        using var context = CreateDbContext();
        var repository = new CustomerRepository(context);
        var customer = new Customer("John", "Doe", "john@example.com", "555-1234");

        // Act
        var result = await repository.CreateAsync(customer);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(customer.Id);

        var savedCustomer = await context.Customers.FindAsync(customer.Id);
        savedCustomer.Should().NotBeNull();
        savedCustomer!.FirstName.Should().Be("John");
        savedCustomer.LastName.Should().Be("Doe");
        savedCustomer.Email.Should().Be("john@example.com");
        savedCustomer.Phone.Should().Be("555-1234");
    }

    [Fact]
    public async Task CreateAsync_WithMinimalCustomer_ShouldPersistCustomer()
    {
        // Arrange
        using var context = CreateDbContext();
        var repository = new CustomerRepository(context);
        var customer = new Customer("Jane", "Smith");

        // Act
        var result = await repository.CreateAsync(customer);

        // Assert
        result.Should().NotBeNull();
        var savedCustomer = await context.Customers.FindAsync(customer.Id);
        savedCustomer.Should().NotBeNull();
        savedCustomer!.Email.Should().BeNull();
        savedCustomer.Phone.Should().BeNull();
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithModifiedCustomer_ShouldUpdateCustomer()
    {
        // Arrange
        using var context = CreateDbContext();
        var repository = new CustomerRepository(context);
        var customer = new Customer("John", "Doe", "john@example.com", "555-1234");
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        // Modify customer
        customer.UpdateFirstName("Jane");
        customer.UpdateLastName("Smith");
        customer.UpdateEmail("jane@example.com");
        customer.UpdatePhone("555-5678");

        // Act
        var result = await repository.UpdateAsync(customer);

        // Assert
        result.Should().NotBeNull();
        var savedCustomer = await context.Customers.FindAsync(customer.Id);
        savedCustomer.Should().NotBeNull();
        savedCustomer!.FirstName.Should().Be("Jane");
        savedCustomer.LastName.Should().Be("Smith");
        savedCustomer.Email.Should().Be("jane@example.com");
        savedCustomer.Phone.Should().Be("555-5678");
    }

    [Fact]
    public async Task UpdateAsync_WithCustomerWithOrders_ShouldPreserveOrders()
    {
        // Arrange
        using var context = CreateDbContext();
        var repository = new CustomerRepository(context);
        var customer = new Customer("John", "Doe");
        var order = new Order(customer.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));

        context.Customers.Add(customer);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        // Modify customer
        customer.UpdateFirstName("Jane");

        // Act
        await repository.UpdateAsync(customer);

        // Assert
        var savedCustomer = await context.Customers
            .Include(c => c.Orders)
            .FirstOrDefaultAsync(c => c.Id == customer.Id);
        savedCustomer.Should().NotBeNull();
        savedCustomer!.Orders.Should().HaveCount(1);
        savedCustomer.Orders.First().Id.Should().Be(order.Id);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithExistingCustomer_ShouldRemoveCustomer()
    {
        // Arrange
        using var context = CreateDbContext();
        var repository = new CustomerRepository(context);
        var customer = new Customer("John", "Doe");
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        // Act
        await repository.DeleteAsync(customer.Id);

        // Assert
        var deletedCustomer = await context.Customers.FindAsync(customer.Id);
        deletedCustomer.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentCustomer_ShouldNotThrow()
    {
        // Arrange
        using var context = CreateDbContext();
        var repository = new CustomerRepository(context);
        var customerId = Guid.NewGuid();

        // Act
        var act = async () => await repository.DeleteAsync(customerId);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DeleteAsync_WithCustomerWithOrders_ShouldThrowException()
    {
        // Arrange
        using var context = CreateDbContext();
        var repository = new CustomerRepository(context);
        var customer = new Customer("John", "Doe");
        var order = new Order(customer.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));

        context.Customers.Add(customer);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        // Act
        var act = async () => await repository.DeleteAsync(customer.Id);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*required relationship*");
        
        // Verify customer still exists
        var existingCustomer = await context.Customers.FindAsync(customer.Id);
        existingCustomer.Should().NotBeNull();
    }

    #endregion
}
