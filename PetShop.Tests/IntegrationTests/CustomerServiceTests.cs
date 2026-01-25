using PetShop.Application.DTOs;
using PetShop.Domain.Exceptions;

namespace PetShop.Tests.IntegrationTests;

public class CustomerServiceTests : IntegrationTestBase
{
    [Fact]
    public async Task CreateCustomerAsync_ShouldCreateCustomerSuccessfully()
    {
        // Arrange
        var newCustomer = new CreateCustomerDto()
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "test@test.com",
            Phone = "123-456-7890",
            Address = "123 Main St, Anytown, USA"
        };

        // Act
        var createdCustomer = await _customerService.CreateCustomerAsync(newCustomer);

        // Assert
        Assert.NotNull(createdCustomer);
        Assert.Equal(newCustomer.FirstName, createdCustomer.FirstName);
        Assert.Equal(newCustomer.LastName, createdCustomer.LastName);
        Assert.Equal(newCustomer.Email, createdCustomer.Email);
        Assert.Equal(newCustomer.Phone, createdCustomer.Phone);
        Assert.Equal(newCustomer.Address, createdCustomer.Address);

        // Verify that the customer is actually in the database
        var fetchedCustomer = await _customerService.GetCustomerAsync(createdCustomer.Id);
        Assert.NotNull(fetchedCustomer);
        Assert.Equal(createdCustomer.Id, fetchedCustomer.Id);
        Assert.Equal(createdCustomer.FirstName, fetchedCustomer.FirstName);
        Assert.Equal(createdCustomer.LastName, fetchedCustomer.LastName);
        Assert.Equal(createdCustomer.Email, fetchedCustomer.Email);
        Assert.Equal(createdCustomer.Phone, fetchedCustomer.Phone);
        Assert.Equal(createdCustomer.Address, fetchedCustomer.Address);
    }

    [Fact]
    public async Task GetCustomerAsync_ShouldReturnCustomer_WhenCustomerExists()
    {
        // Arrange
        var testCustomer = await CreateCustomerTestAsync();

        // Act
        var fetchedCustomer = await _customerService.GetCustomerAsync(testCustomer.Id);
        
        // Assert
        Assert.NotNull(fetchedCustomer);
        Assert.Equal(testCustomer.Id, fetchedCustomer.Id);
        Assert.Equal(testCustomer.FirstName, fetchedCustomer.FirstName);
        Assert.Equal(testCustomer.LastName, fetchedCustomer.LastName);
        Assert.Equal(testCustomer.Email, fetchedCustomer.Email);
        Assert.Equal(testCustomer.Phone, fetchedCustomer.Phone);
        Assert.Equal(testCustomer.Address, fetchedCustomer.Address);
    }

    [Fact]
    public async Task GetCustomerAsync_ShouldReturnNull_WhenCustomerDoesNotExist()
    {
        // Arrange
        var nonExistentCustomerId = Guid.NewGuid();

        // Act + Assert
        var ex = await Assert.ThrowsAsync<AppException>(() => _customerService.GetCustomerAsync(nonExistentCustomerId));
        Assert.Equal($"Customer not found for update", ex.Message);
    }

    [Fact]
    public async Task GetAllCustomersAsync_ShouldReturnAllCustomers()
    {
        // Arrange
        var newCustomer = new CreateCustomerDto()
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "test@test.com",
            Phone = "123-456-7890",
            Address = "123 Main St, Anytown, USA"
        };
        
        var customer1 = await _customerService.CreateCustomerAsync(newCustomer);
        var customer2 = await CreateCustomerTestAsync();

        // Act
        var allCustomers = await _customerService.GetAllCustomersAsync();

        // Assert
        Assert.NotNull(allCustomers);
        var customerList = allCustomers.ToList();
        Assert.True(customerList.Count >= 2);
        Assert.Contains(customerList, c => c.Id == customer1.Id);
        Assert.Contains(customerList, c => c.Id == customer2.Id);

    }

    [Fact]
    public async Task DeleteCustomerAsync_ShouldDeleteCustomerSuccessfully()
    {
        // Arrange
        var testCustomer = await CreateCustomerTestAsync();

        // Act
        var deleteResult = await _customerService.DeleteCustomerAsync(testCustomer.Id);

        // Assert
        Assert.NotNull(deleteResult);
        Assert.Equal(testCustomer.Id, deleteResult.Id);
        
        // Act + Assert
        var ex = await Assert.ThrowsAsync<AppException>(() => _customerService.GetCustomerAsync(testCustomer.Id));
        Assert.Equal($"Customer not found for update", ex.Message);
    }
    
    [Fact]
    public async Task DeleteCustomerAsync_ShouldThrowException_WhenCustomerDoesNotExist()
    {
        // Arrange
        var nonExistentCustomerId = Guid.NewGuid();
        
        // Act + Assert
        var ex = await Assert.ThrowsAsync<AppException>(() => _customerService.DeleteCustomerAsync(nonExistentCustomerId));
        Assert.Equal($"Customer with id '{nonExistentCustomerId}' not found.", ex.Message);
    }

    [Fact]
    public async Task UpdateCustomerAsync_ShouldUpdateCustomerSuccessfully()
    {
        // Arrange
        var testCustomer = await CreateCustomerTestAsync();
        var updateCustomerDto = new UpdateCustomerDto()
        {
            FirstName = "UpdatedFirstName",
            LastName = "UpdatedLastName",
        };
        // Act
        var updatedCustomer = await _customerService.UpdateCustomerAsync(testCustomer.Id, updateCustomerDto);
        // Assert
        Assert.NotNull(updatedCustomer);
        Assert.Equal(testCustomer.Id, updatedCustomer.Id);
        Assert.Equal(updateCustomerDto.FirstName, updatedCustomer.FirstName);
        Assert.Equal(updateCustomerDto.LastName, updatedCustomer.LastName);

    }

    [Fact]
    public async Task UpdateCustomerAsync_ShouldReturnNull_WhenCustomerDoesNotExist()
    {
        // Arrange
        var nonExistentCustomerId = Guid.NewGuid();
        var updateCustomerDto = new UpdateCustomerDto()
        {
            FirstName = "UpdatedFirstName",
            LastName = "UpdatedLastName",
        };
        // Act + Assert
        var ex = await Assert.ThrowsAsync<AppException>(() => _customerService.UpdateCustomerAsync(nonExistentCustomerId, updateCustomerDto));
        Assert.Equal($"Customer not found for update", ex.Message);
    }
    
    [Fact] //new crud workflow
    public async Task CustomerCrudWorkflow_ShouldWorkCorrectly()
    {
        // Create Customer
        var newCustomer = new CreateCustomerDto()
        {
            FirstName = "Alice",
            LastName = "Smith",
            Email = "",
            Phone = "987-654-3210",
            Address = "456 Another St, Sometown, USA"
        };
        
        var createdCustomer = await _customerService.CreateCustomerAsync(newCustomer);
        Assert.NotNull(createdCustomer);
        
        // Update Customer
        var updateCustomerDto = new UpdateCustomerDto()
        {
            FirstName = "AliceUpdated",
            LastName = "SmithUpdated",
        };
        
        var updatedCustomer = await _customerService.UpdateCustomerAsync(createdCustomer.Id, updateCustomerDto);
        Assert.NotNull(updatedCustomer);
        Assert.Equal("AliceUpdated", updatedCustomer.FirstName);
        Assert.Equal("SmithUpdated", updatedCustomer.LastName);
        
        // Get Customer
        var fetchedCustomer = await _customerService.GetCustomerAsync(createdCustomer.Id);
        Assert.NotNull(fetchedCustomer);
        Assert.Equal("AliceUpdated", fetchedCustomer.FirstName);
        Assert.Equal("SmithUpdated", fetchedCustomer.LastName); 
        
        // Delete Customer
        var deletedCustomer = await _customerService.DeleteCustomerAsync(createdCustomer.Id);
        Assert.NotNull(deletedCustomer);
        
        // Verify Deletion
        var ex = await Assert.ThrowsAsync<AppException>(() => _customerService.GetCustomerAsync(createdCustomer.Id));
        Assert.Equal($"Customer not found for update", ex.Message);
    }
    [Fact]
    public async Task CreateCustomerAsync_ShouldThrowException_WhenEmailIsNotUnique()
    {
        // Arrange
        var existingCustomer = await CreateCustomerTestAsync();
        
        var newCustomer = new CreateCustomerDto()
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = existingCustomer.Email, // Duplicate email
            Phone = "098-765-4321",
            Address = "456 Another St, Othertown, USA"
        };
        
        // Act + Assert
        var ex = await Assert.ThrowsAsync<AppException>(() => _customerService.CreateCustomerAsync(newCustomer));
        Assert.Equal($"A customer with the email '{existingCustomer.Email}' already exists.", ex.Message);
    }
}