using Moq;
using PetShop.Application.DTOs;
using PetShop.Application.Features.Customer.Interfaces;
using PetShop.Application.Features.Customer.Services;
using PetShop.Application.Mappers;
using PetShop.Domain.Entities;
using PetShop.Domain.Interfaces;
using Xunit;

namespace PetShop.Test;

public class CustomerServiceTests
{
    private readonly Mock<ICustomerRepository> _mockRepository;
    private readonly Mock<ICustomerMapper> _mockMapper;
    private readonly CustomerService _customerService;

    public CustomerServiceTests()
    {
        _mockRepository = new Mock<ICustomerRepository>();
        _mockMapper = new Mock<ICustomerMapper>();
        _customerService = new CustomerService(_mockRepository.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task CreateCustomerAsync_ValidData_ReturnsCustomerDto()
    {
        // Arrange
        var createCustomerDto = new CreateCustomerDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Phone = "123-456-7890",
            Address = "123 Main St"
        };

        var customer = new Customer
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Phone = "123-456-7890",
            Address = "123 Main St"
        };

        var expectedCustomerDto = new CustomerDto
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Phone = "123-456-7890",
            Address = "123 Main St"
        };

        _mockMapper.Setup(m => m.ToDomain(createCustomerDto)).Returns(customer);
        _mockMapper.Setup(m => m.ToDto(customer)).Returns(expectedCustomerDto);
        _mockRepository.Setup(r => r.CreateCustomerAsync(customer)).ReturnsAsync(customer);

        // Act
        var result = await _customerService.CreateCustomerAsync(createCustomerDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedCustomerDto.Id, result.Id);
        Assert.Equal(expectedCustomerDto.FirstName, result.FirstName);
        Assert.Equal(expectedCustomerDto.LastName, result.LastName);
        Assert.Equal(expectedCustomerDto.Email, result.Email);
        Assert.Equal(expectedCustomerDto.Phone, result.Phone);
        Assert.Equal(expectedCustomerDto.Address, result.Address);

        _mockMapper.Verify(m => m.ToDomain(createCustomerDto), Times.Once);
        _mockMapper.Verify(m => m.ToDto(customer), Times.Once);
        _mockRepository.Verify(r => r.CreateCustomerAsync(customer), Times.Once);
    }

    [Fact]
    public async Task GetCustomerAsync_ExistingCustomer_ReturnsCustomerDto()
    {
        // Arrange
        var customerId = 1;
        var customer = new Customer
        {
            Id = customerId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Phone = "123-456-7890",
            Address = "123 Main St"
        };

        var expectedCustomerDto = new CustomerDto
        {
            Id = customerId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Phone = "123-456-7890",
            Address = "123 Main St"
        };

        _mockRepository.Setup(r => r.GetCustomerByIdAsync(customerId)).ReturnsAsync(customer);
        _mockMapper.Setup(m => m.ToDto(customer)).Returns(expectedCustomerDto);

        // Act
        var result = await _customerService.GetCustomerAsync(customerId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedCustomerDto.Id, result.Id);
        Assert.Equal(expectedCustomerDto.FirstName, result.FirstName);
        Assert.Equal(expectedCustomerDto.LastName, result.LastName);

        _mockRepository.Verify(r => r.GetCustomerByIdAsync(customerId), Times.Once);
        _mockMapper.Verify(m => m.ToDto(customer), Times.Once);
    }

    [Fact]
    public async Task GetCustomerAsync_NonExistingCustomer_ReturnsNull()
    {
        // Arrange
        var customerId = 999;
        _mockRepository.Setup(r => r.GetCustomerByIdAsync(customerId)).ReturnsAsync((Customer?)null);

        // Act
        var result = await _customerService.GetCustomerAsync(customerId);

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(r => r.GetCustomerByIdAsync(customerId), Times.Once);
        _mockMapper.Verify(m => m.ToDto(It.IsAny<Customer>()), Times.Never);
    }

    [Fact]
    public async Task GetAllCustomersAsync_ReturnsAllCustomers()
    {
        // Arrange
        var customers = new List<Customer>
        {
            new Customer { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com" },
            new Customer { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane@example.com" }
        };

        var expectedCustomerDtos = new List<CustomerDto>
        {
            new CustomerDto { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com" },
            new CustomerDto { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane@example.com" }
        };

        _mockRepository.Setup(r => r.GetAllCustomersAsync()).ReturnsAsync(customers);
        _mockMapper.Setup(m => m.ToDto(customers[0])).Returns(expectedCustomerDtos[0]);
        _mockMapper.Setup(m => m.ToDto(customers[1])).Returns(expectedCustomerDtos[1]);

        // Act
        var result = await _customerService.GetAllCustomersAsync();

        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.Equal(expectedCustomerDtos[0].Id, resultList[0].Id);
        Assert.Equal(expectedCustomerDtos[1].Id, resultList[1].Id);

        _mockRepository.Verify(r => r.GetAllCustomersAsync(), Times.Once);
        _mockMapper.Verify(m => m.ToDto(It.IsAny<Customer>()), Times.Exactly(2));
    }

    [Fact]
    public async Task UpdateCustomerAsync_ExistingCustomer_ReturnsUpdatedCustomerDto()
    {
        // Arrange
        var customerId = 1;
        var updateCustomerDto = new UpdateCustomerDto
        {
            FirstName = "John Updated",
            LastName = "Doe Updated",
            Email = "john.updated@example.com",
            Phone = "987-654-3210",
            Address = "456 Updated St"
        };

        var customer = new Customer
        {
            Id = customerId,
            FirstName = "John Updated",
            LastName = "Doe Updated",
            Email = "john.updated@example.com",
            Phone = "987-654-3210",
            Address = "456 Updated St"
        };

        var expectedCustomerDto = new CustomerDto
        {
            Id = customerId,
            FirstName = "John Updated",
            LastName = "Doe Updated",
            Email = "john.updated@example.com",
            Phone = "987-654-3210",
            Address = "456 Updated St"
        };

        _mockMapper.Setup(m => m.ToDomain(updateCustomerDto)).Returns(customer);
        _mockRepository.Setup(r => r.UpdateCustomerAsync(customer)).ReturnsAsync(customer);
        _mockMapper.Setup(m => m.ToDto(customer)).Returns(expectedCustomerDto);

        // Act
        var result = await _customerService.UpdateCustomerAsync(customerId, updateCustomerDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedCustomerDto.Id, result.Id);
        Assert.Equal(expectedCustomerDto.FirstName, result.FirstName);
        Assert.Equal(expectedCustomerDto.LastName, result.LastName);

        _mockMapper.Verify(m => m.ToDomain(updateCustomerDto), Times.Once);
        _mockRepository.Verify(r => r.UpdateCustomerAsync(customer), Times.Once);
        _mockMapper.Verify(m => m.ToDto(customer), Times.Once);
    }

    [Fact]
    public async Task UpdateCustomerAsync_NonExistingCustomer_ReturnsNull()
    {
        // Arrange
        var customerId = 999;
        var updateCustomerDto = new UpdateCustomerDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        var customer = new Customer
        {
            Id = customerId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        _mockMapper.Setup(m => m.ToDomain(updateCustomerDto)).Returns(customer);
        _mockRepository.Setup(r => r.UpdateCustomerAsync(customer)).ReturnsAsync((Customer?)null);

        // Act
        var result = await _customerService.UpdateCustomerAsync(customerId, updateCustomerDto);

        // Assert
        Assert.Null(result);
        _mockMapper.Verify(m => m.ToDomain(updateCustomerDto), Times.Once);
        _mockRepository.Verify(r => r.UpdateCustomerAsync(customer), Times.Once);
        _mockMapper.Verify(m => m.ToDto(It.IsAny<Customer>()), Times.Never);
    }

    [Fact]
    public async Task DeleteCustomerAsync_ExistingCustomer_ReturnsDeletedCustomerDto()
    {
        // Arrange
        var customerId = 1;
        var customer = new Customer
        {
            Id = customerId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        var expectedCustomerDto = new CustomerDto
        {
            Id = customerId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        _mockRepository.Setup(r => r.DeleteCustomerByIdAsync(customerId)).ReturnsAsync(customer);
        _mockMapper.Setup(m => m.ToDto(customer)).Returns(expectedCustomerDto);

        // Act
        var result = await _customerService.DeleteCustomerAsync(customerId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedCustomerDto.Id, result.Id);
        Assert.Equal(expectedCustomerDto.FirstName, result.FirstName);
        Assert.Equal(expectedCustomerDto.LastName, result.LastName);

        _mockRepository.Verify(r => r.DeleteCustomerByIdAsync(customerId), Times.Once);
        _mockMapper.Verify(m => m.ToDto(customer), Times.Once);
    }

    [Fact]
    public async Task DeleteCustomerAsync_NonExistingCustomer_ReturnsNull()
    {
        // Arrange
        var customerId = 999;
        _mockRepository.Setup(r => r.DeleteCustomerByIdAsync(customerId)).ReturnsAsync((Customer?)null);

        // Act
        var result = await _customerService.DeleteCustomerAsync(customerId);

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(r => r.DeleteCustomerByIdAsync(customerId), Times.Once);
        _mockMapper.Verify(m => m.ToDto(It.IsAny<Customer>()), Times.Never);
    }
}
