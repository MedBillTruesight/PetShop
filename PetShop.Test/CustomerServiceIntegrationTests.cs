using PetShop.Application.DTOs;
using Xunit;

namespace PetShop.Test;

public class CustomerServiceIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task CreateCustomerAsync_ValidData_CreatesCustomerInDatabase()
    {
        // Arrange
        var createCustomerDto = new CreateCustomerDto
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            Phone = "987-654-3210",
            Address = "456 Oak St"
        };

        // Act
        var result = await _customerService.CreateCustomerAsync(createCustomerDto);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(0, result.Id);
        Assert.Equal(createCustomerDto.FirstName, result.FirstName);
        Assert.Equal(createCustomerDto.LastName, result.LastName);
        Assert.Equal(createCustomerDto.Email, result.Email);
        Assert.Equal(createCustomerDto.Phone, result.Phone);
        Assert.Equal(createCustomerDto.Address, result.Address);

        // Verify customer was saved to database
        var savedCustomer = await _customerService.GetCustomerAsync(result.Id);
        Assert.NotNull(savedCustomer);
        Assert.Equal(result.Id, savedCustomer.Id);
        Assert.Equal(result.FirstName, savedCustomer.FirstName);
    }

    [Fact]
    public async Task GetCustomerAsync_ExistingCustomer_ReturnsCustomer()
    {
        // Arrange
        var customer = await CreateTestCustomerAsync();

        // Act
        var result = await _customerService.GetCustomerAsync(customer.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(customer.Id, result.Id);
        Assert.Equal(customer.FirstName, result.FirstName);
        Assert.Equal(customer.LastName, result.LastName);
        Assert.Equal(customer.Email, result.Email);
    }

    [Fact]
    public async Task GetCustomerAsync_NonExistingCustomer_ReturnsNull()
    {
        // Act
        var result = await _customerService.GetCustomerAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllCustomersAsync_ReturnsAllCustomers()
    {
        // Arrange
        var customer1 = await CreateTestCustomerAsync();
        var customer2 = await CreateTestCustomerAsync();

        // Act
        var result = await _customerService.GetAllCustomersAsync();

        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();
        Assert.True(resultList.Count >= 2);
        Assert.Contains(resultList, c => c.Id == customer1.Id);
        Assert.Contains(resultList, c => c.Id == customer2.Id);
    }

    [Fact]
    public async Task UpdateCustomerAsync_ExistingCustomer_UpdatesCustomerInDatabase()
    {
        // Arrange
        var customer = await CreateTestCustomerAsync();
        var updateCustomerDto = new UpdateCustomerDto
        {
            FirstName = "Jane Updated",
            LastName = "Smith Updated",
            Email = "jane.updated@example.com",
            Phone = "111-222-3333",
            Address = "789 Updated St"
        };

        // Act
        var result = await _customerService.UpdateCustomerAsync(customer.Id, updateCustomerDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(customer.Id, result.Id);
        Assert.Equal(updateCustomerDto.FirstName, result.FirstName);
        Assert.Equal(updateCustomerDto.LastName, result.LastName);
        Assert.Equal(updateCustomerDto.Email, result.Email);
        Assert.Equal(updateCustomerDto.Phone, result.Phone);
        Assert.Equal(updateCustomerDto.Address, result.Address);

        // Verify customer was updated in database
        var updatedCustomer = await _customerService.GetCustomerAsync(customer.Id);
        Assert.NotNull(updatedCustomer);
        Assert.Equal(updateCustomerDto.FirstName, updatedCustomer.FirstName);
        Assert.Equal(updateCustomerDto.LastName, updatedCustomer.LastName);
    }

    [Fact]
    public async Task UpdateCustomerAsync_NonExistingCustomer_ReturnsNull()
    {
        // Arrange
        var updateCustomerDto = new UpdateCustomerDto
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane@example.com"
        };

        // Act
        var result = await _customerService.UpdateCustomerAsync(999, updateCustomerDto);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteCustomerAsync_ExistingCustomer_DeletesCustomerFromDatabase()
    {
        // Arrange
        var customer = await CreateTestCustomerAsync();

        // Act
        var result = await _customerService.DeleteCustomerAsync(customer.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(customer.Id, result.Id);
        Assert.Equal(customer.FirstName, result.FirstName);
        Assert.Equal(customer.LastName, result.LastName);

        // Verify customer was deleted from database
        var deletedCustomer = await _customerService.GetCustomerAsync(customer.Id);
        Assert.Null(deletedCustomer);
    }

    [Fact]
    public async Task DeleteCustomerAsync_NonExistingCustomer_ReturnsNull()
    {
        // Act
        var result = await _customerService.DeleteCustomerAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CustomerCRUD_CompleteWorkflow_Succeeds()
    {
        // Create
        var createCustomerDto = new CreateCustomerDto
        {
            FirstName = "Alice",
            LastName = "Johnson",
            Email = "alice.johnson@example.com",
            Phone = "555-123-4567",
            Address = "321 Pine St"
        };

        var createdCustomer = await _customerService.CreateCustomerAsync(createCustomerDto);
        Assert.NotNull(createdCustomer);
        Assert.NotEqual(0, createdCustomer.Id);

        // Read
        var retrievedCustomer = await _customerService.GetCustomerAsync(createdCustomer.Id);
        Assert.NotNull(retrievedCustomer);
        Assert.Equal(createdCustomer.Id, retrievedCustomer.Id);
        Assert.Equal(createCustomerDto.FirstName, retrievedCustomer.FirstName);

        // Update
        var updateCustomerDto = new UpdateCustomerDto
        {
            FirstName = "Alice Updated",
            LastName = "Johnson Updated",
            Email = "alice.updated@example.com",
            Phone = "555-987-6543",
            Address = "654 Updated St"
        };

        var updatedCustomer = await _customerService.UpdateCustomerAsync(createdCustomer.Id, updateCustomerDto);
        Assert.NotNull(updatedCustomer);
        Assert.Equal(updateCustomerDto.FirstName, updatedCustomer.FirstName);
        Assert.Equal(updateCustomerDto.LastName, updatedCustomer.LastName);

        // Verify update
        var verifyUpdatedCustomer = await _customerService.GetCustomerAsync(createdCustomer.Id);
        Assert.NotNull(verifyUpdatedCustomer);
        Assert.Equal(updateCustomerDto.FirstName, verifyUpdatedCustomer.FirstName);

        // Delete
        var deletedCustomer = await _customerService.DeleteCustomerAsync(createdCustomer.Id);
        Assert.NotNull(deletedCustomer);
        Assert.Equal(createdCustomer.Id, deletedCustomer.Id);

        // Verify deletion
        var verifyDeletedCustomer = await _customerService.GetCustomerAsync(createdCustomer.Id);
        Assert.Null(verifyDeletedCustomer);
    }
} 