# Test Utilities

This directory contains reusable test utilities for the PetShop test suite. These utilities help create consistent, maintainable tests across all layers of the application.

## Directory Structure

```
TestUtilities/
├── Builders/           # Fluent builders for domain entities
├── Fixtures/           # Pre-configured test data fixtures
├── Factories/          # Test data generation factories
├── Api/               # API testing utilities
├── Base/              # Base test classes
└── README.md          # This documentation
```

## Builders

Fluent builders provide a clean, readable API for constructing test data.

### CustomerBuilder

```csharp
// Basic customer
var customer = new CustomerBuilder()
    .WithFirstName("John")
    .WithLastName("Doe")
    .Build();

// Customer with complete info
var customer = new CustomerBuilder()
    .WithCompleteInfo()
    .Build();

// Customer with email only
var customer = new CustomerBuilder()
    .WithMinimalInfo()
    .WithEmail("test@example.com")
    .Build();
```

### OrderBuilder

```csharp
// Empty order
var order = new OrderBuilder()
    .WithCustomerId(customerId)
    .WithPickupDateInDays(1)
    .Build();

// Order with pets
var order = new OrderBuilder()
    .WithCustomerId(customerId)
    .WithPickupDateInDays(2)
    .WithPet("Fluffy", 100m, "Cat", "White")
    .WithPet("Buddy", 150m, "Dog", "Brown")
    .Build();

// Order ready for processing
var order = new OrderBuilder()
    .WithCustomerId(customerId)
    .ReadyForProcessing()
    .Build();

// Order ready for delivery
var order = new OrderBuilder()
    .WithCustomerId(customerId)
    .ReadyForDelivery()
    .Build();
```

### PetBuilder

```csharp
// Basic pet
var pet = new PetBuilder()
    .WithOrderId(orderId)
    .WithMinimalInfo()
    .Build();

// Complete pet
var pet = new PetBuilder()
    .WithOrderId(orderId)
    .WithCompleteInfo()
    .Build();

// Pet with specific characteristics
var pet = new PetBuilder()
    .WithOrderId(orderId)
    .AsDog()
    .WithPrice(200m)
    .WithColor("Golden")
    .Build();
```

## Fixtures

Fixtures provide pre-configured entities for common test scenarios.

```csharp
public class MyTests : BaseTest
{
    [Fact]
    public void Test_With_Basic_Customer()
    {
        var customer = CustomerFixture.BasicCustomer;
        // Use customer in test
    }

    [Fact]
    public void Test_With_Order_Scenarios()
    {
        var emptyOrder = OrderFixture.EmptyOrder;
        var orderWithPets = OrderFixture.OrderWithMultiplePets;
        var deliveredOrder = OrderFixture.DeliveredOrder;
        // Use orders in test
    }
}
```

## Base Test Classes

### BaseTest

Provides common functionality for all tests:

```csharp
public class MyTests : BaseTest
{
    [Fact]
    public void My_Test()
    {
        // Access to fixtures
        var customer = CustomerFixture.BasicCustomer;
        var order = OrderFixture.EmptyOrder;

        // Utility methods
        var id = NewGuid();
        var today = Today;
        var tomorrow = Tomorrow;

        // Assertion helpers
        AssertThrows<BusinessRuleViolationException>(() =>
            new Customer("", "Doe"));
    }
}
```

### BaseRepositoryTest

Provides database setup for repository tests:

```csharp
public class MyRepositoryTests : BaseRepositoryTest
{
    [Fact]
    public async Task My_Repository_Test()
    {
        // Create test data
        var customer = await CreateAndSaveCustomerAsync("John", "Doe");
        var order = await CreateAndSaveOrderAsync(
            customer.Id,
            Tomorrow,
            CreateTestPet(customer.Id, "Fluffy", 100m));

        // Test repository methods
        // Database is automatically cleaned up
    }
}
```

### BaseApiTest

Provides HTTP client setup for API tests:

```csharp
public class MyApiTests : BaseApiTest
{
    public MyApiTests(TestServerFixture fixture) : base(fixture) { }

    [Fact]
    public async Task My_Api_Test()
    {
        // Create test data via API
        var customer = await CreateTestCustomerAsync("John", "Doe");
        var order = await CreateTestOrderAsync(
            customer.Id,
            Tomorrow,
            CreatePetRequest("Fluffy", 100m, "Cat"));

        // Test API endpoints
        var response = await Client.GetAsync($"/api/v1/customers/{customer.Id}");
        AssertSuccess(response);
    }
}
```

## Test Data Factory

Generates consistent test data across different tests:

```csharp
public class MyTests
{
    [Fact]
    public void Test_With_Factory_Data()
    {
        // Create unique entities
        var customer = TestDataFactory.CreateCustomer();
        var order = TestDataFactory.CreateOrderWithCustomer(petCount: 2);

        // Create delivered order
        var deliveredOrder = TestDataFactory.CreateDeliveredOrder();

        // Create multiple entities
        var customers = TestDataFactory.CreateCustomers(5);
        var orders = TestDataFactory.CreateOrdersForCustomer(customer.Id, 3);
    }
}
```

## Best Practices

1. **Use Builders for Complex Objects**: When you need entities with specific properties
2. **Use Fixtures for Common Scenarios**: When you need pre-defined test data
3. **Use Base Classes**: Inherit from appropriate base class for setup/teardown
4. **Use Factory for Bulk Data**: When creating multiple similar entities
5. **Reset Factory Sequence**: In tests requiring deterministic data

## Examples

See existing test files for usage examples:
- `CustomerTests.cs` - Uses builders and fixtures
- `CustomerServiceTests.cs` - Uses base test class
- `CustomerRepositoryTests.cs` - Uses repository base class
- `CustomersControllerTests.cs` - Uses API base class