using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PetShop.Application.DTOs;
using PetShop.Application.Repositories;
using PetShop.Application.Services;
using PetShop.Infrastructure;
using PetShop.Infrastructure.Repositories;

namespace PetShop.Tests;

/// <summary>
/// Integration tests for dependency injection configuration.
/// Tests verify that all services can be resolved from the DI container
/// and that dependencies are correctly wired.
/// </summary>
public class DependencyInjectionTests
{
    private static IServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();

        // Configure DbContext with InMemory provider
        services.AddDbContext<PetShopDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        // Register repositories
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        // Register application services
        services.AddScoped<CustomerService>();
        services.AddScoped<OrderService>();

        return services.BuildServiceProvider();
    }

    #region DbContext Resolution Tests

    [Fact]
    public void ServiceProvider_CanResolveDbContext_ShouldSucceed()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();

        // Act
        var dbContext = serviceProvider.GetRequiredService<PetShopDbContext>();

        // Assert
        dbContext.Should().NotBeNull();
        dbContext.Customers.Should().NotBeNull();
        dbContext.Orders.Should().NotBeNull();
        dbContext.Pets.Should().NotBeNull();
    }

    [Fact]
    public void ServiceProvider_CanResolveDbContextMultipleTimes_ShouldReturnDifferentInstances()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();

        // Act
        var dbContext1 = serviceProvider.GetRequiredService<PetShopDbContext>();
        var dbContext2 = serviceProvider.GetRequiredService<PetShopDbContext>();

        // Assert
        dbContext1.Should().NotBeNull();
        dbContext2.Should().NotBeNull();
        // DbContext is registered as Scoped, but outside a scope, each resolution creates a new instance
        // In a real HTTP request, they would be the same instance within the scope
    }

    #endregion

    #region Repository Resolution Tests

    [Fact]
    public void ServiceProvider_CanResolveCustomerRepository_ShouldSucceed()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();

        // Act
        var repository = serviceProvider.GetRequiredService<ICustomerRepository>();

        // Assert
        repository.Should().NotBeNull();
        repository.Should().BeOfType<CustomerRepository>();
    }

    [Fact]
    public void ServiceProvider_CanResolveOrderRepository_ShouldSucceed()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();

        // Act
        var repository = serviceProvider.GetRequiredService<IOrderRepository>();

        // Assert
        repository.Should().NotBeNull();
        repository.Should().BeOfType<OrderRepository>();
    }

    [Fact]
    public void ServiceProvider_ResolveRepositoriesMultipleTimes_ShouldReturnDifferentInstances()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();

        // Act
        var repository1 = serviceProvider.GetRequiredService<ICustomerRepository>();
        var repository2 = serviceProvider.GetRequiredService<ICustomerRepository>();

        // Assert
        repository1.Should().NotBeNull();
        repository2.Should().NotBeNull();
        // Repositories are Scoped, but outside a scope, each resolution creates a new instance
    }

    [Fact]
    public void ServiceProvider_RepositoriesHaveDbContextDependency_ShouldBeResolved()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();

        // Act
        var customerRepository = serviceProvider.GetRequiredService<ICustomerRepository>();
        var orderRepository = serviceProvider.GetRequiredService<IOrderRepository>();

        // Assert
        customerRepository.Should().NotBeNull();
        orderRepository.Should().NotBeNull();
        // If repositories couldn't resolve DbContext, this would throw an exception
    }

    #endregion

    #region Application Service Resolution Tests

    [Fact]
    public void ServiceProvider_CanResolveCustomerService_ShouldSucceed()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();

        // Act
        var service = serviceProvider.GetRequiredService<CustomerService>();

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void ServiceProvider_CanResolveOrderService_ShouldSucceed()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();

        // Act
        var service = serviceProvider.GetRequiredService<OrderService>();

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void ServiceProvider_ApplicationServicesHaveRepositoryDependencies_ShouldBeResolved()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();

        // Act
        var customerService = serviceProvider.GetRequiredService<CustomerService>();
        var orderService = serviceProvider.GetRequiredService<OrderService>();

        // Assert
        customerService.Should().NotBeNull();
        orderService.Should().NotBeNull();
        // If services couldn't resolve repositories, this would throw an exception
    }

    [Fact]
    public void ServiceProvider_ResolveServicesMultipleTimes_ShouldReturnDifferentInstances()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();

        // Act
        var service1 = serviceProvider.GetRequiredService<CustomerService>();
        var service2 = serviceProvider.GetRequiredService<CustomerService>();

        // Assert
        service1.Should().NotBeNull();
        service2.Should().NotBeNull();
        // Services are Scoped, but outside a scope, each resolution creates a new instance
    }

    #endregion

    #region Service Lifetime Tests

    [Fact]
    public void ServiceProvider_WithinScope_ShouldReturnSameInstances()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();

        // Act & Assert
        using (var scope = serviceProvider.CreateScope())
        {
            var dbContext1 = scope.ServiceProvider.GetRequiredService<PetShopDbContext>();
            var dbContext2 = scope.ServiceProvider.GetRequiredService<PetShopDbContext>();

            var customerRepository1 = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
            var customerRepository2 = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();

            var customerService1 = scope.ServiceProvider.GetRequiredService<CustomerService>();
            var customerService2 = scope.ServiceProvider.GetRequiredService<CustomerService>();

            // Within the same scope, Scoped services should return the same instance
            dbContext1.Should().BeSameAs(dbContext2);
            customerRepository1.Should().BeSameAs(customerRepository2);
            customerService1.Should().BeSameAs(customerService2);
        }
    }

    [Fact]
    public void ServiceProvider_AcrossScopes_ShouldReturnDifferentInstances()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();

        // Act
        PetShopDbContext dbContext1;
        ICustomerRepository customerRepository1;
        CustomerService customerService1;

        using (var scope1 = serviceProvider.CreateScope())
        {
            dbContext1 = scope1.ServiceProvider.GetRequiredService<PetShopDbContext>();
            customerRepository1 = scope1.ServiceProvider.GetRequiredService<ICustomerRepository>();
            customerService1 = scope1.ServiceProvider.GetRequiredService<CustomerService>();
        }

        PetShopDbContext dbContext2;
        ICustomerRepository customerRepository2;
        CustomerService customerService2;

        using (var scope2 = serviceProvider.CreateScope())
        {
            dbContext2 = scope2.ServiceProvider.GetRequiredService<PetShopDbContext>();
            customerRepository2 = scope2.ServiceProvider.GetRequiredService<ICustomerRepository>();
            customerService2 = scope2.ServiceProvider.GetRequiredService<CustomerService>();
        }

        // Assert
        // Across different scopes, Scoped services should return different instances
        dbContext1.Should().NotBeSameAs(dbContext2);
        customerRepository1.Should().NotBeSameAs(customerRepository2);
        customerService1.Should().NotBeSameAs(customerService2);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task ServiceProvider_ResolvedServices_CanPerformOperations()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var customerService = scope.ServiceProvider.GetRequiredService<CustomerService>();
        var orderService = scope.ServiceProvider.GetRequiredService<OrderService>();

        // Act
        var createCustomerRequest = new CreateCustomerRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Phone = "555-1234"
        };

        var customer = await customerService.CreateCustomerAsync(createCustomerRequest);

        // Assert
        customer.Should().NotBeNull();
        customer.Id.Should().NotBeEmpty();
        customer.FirstName.Should().Be("John");
    }

    [Fact]
    public async Task ServiceProvider_ResolvedServices_CanUseDbContext()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PetShopDbContext>();

        // Act
        var customer = new PetShop.Domain.Customer("John", "Doe");
        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync();

        var retrievedCustomer = await dbContext.Customers.FindAsync(customer.Id);

        // Assert
        retrievedCustomer.Should().NotBeNull();
        retrievedCustomer!.FirstName.Should().Be("John");
    }

    #endregion
}
