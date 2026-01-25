using Moq;
using PetShop.Application.DTOs;
using PetShop.Application.Features.Customers;
using PetShop.Application.Interfaces.Mappers;
using PetShop.Domain.Entities;
using PetShop.Domain.Interfaces.Repositories;
using PetShop.Domain.Exceptions;

namespace PetShop.Tests.UnitTests;
public class CustomerServiceTests
{
    private readonly CustomerService _customerService;
    private readonly Mock<ICustomerRepository> _customerRepository;
    private readonly Mock<ICustomerMapper> _customerMapper;

    public CustomerServiceTests()
    {
        _customerRepository = new Mock<ICustomerRepository>();
        _customerMapper = new Mock<ICustomerMapper>();
        _customerService = new CustomerService(_customerRepository.Object, _customerMapper.Object);
    }

    [Fact]
    public async Task CreateCustomerAsync_ShouldCreateCustomerSuccessfully_AndReturnDto()
    {
        // Arrange
        var createCustomerDto = new CreateCustomerDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "test@test.com",
            Phone = "123-456-7890",
            Address = "123 Main St, Anytown, USA"
        };

        var customerDomain = new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = createCustomerDto.FirstName,
            LastName = createCustomerDto.LastName,
            Email = createCustomerDto.Email,
            Phone = createCustomerDto.Phone,
            Address = createCustomerDto.Address
        };

        var customerDto = new CustomerDto
        {
            Id = customerDomain.Id,
            FirstName = customerDomain.FirstName,
            LastName = customerDomain.LastName,
            Email = customerDomain.Email,
            Phone = customerDomain.Phone,
            Address = customerDomain.Address
        };

        _customerMapper.Setup(m => m.ToDomain(createCustomerDto)).Returns(customerDomain);
        _customerMapper.Setup(m => m.ToDto(customerDomain)).Returns(customerDto);

        // Ensure email uniqueness check has a non-null source
        _customerRepository.Setup(r => r.GetAllCustomersAsync()).ReturnsAsync(new List<Customer>());

        _customerRepository.Setup(r => r.CreateCustomerAsync(customerDomain)).ReturnsAsync(customerDomain);

        // Act
        var result = await _customerService.CreateCustomerAsync(createCustomerDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(customerDto.Id, result.Id);
        Assert.Equal(customerDto.FirstName, result.FirstName);
        Assert.Equal(customerDto.LastName, result.LastName);
        Assert.Equal(customerDto.Email, result.Email);
        Assert.Equal(customerDto.Phone, result.Phone);
        Assert.Equal(customerDto.Address, result.Address);

        _customerMapper.Verify(m => m.ToDomain(createCustomerDto), Times.Once);
        _customerMapper.Verify(m => m.ToDto(customerDomain), Times.Once);
        _customerRepository.Verify(r => r.GetAllCustomersAsync(), Times.Once);
        _customerRepository.Verify(r => r.CreateCustomerAsync(customerDomain), Times.Once);
    }

    [Fact]
    public async Task GetCustomerAsync_ShouldReturnCustomerDto_WhenCustomerExists()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customerDomain = new Customer
        {
            Id = customerId,
            FirstName = "Jane",
            LastName = "Doe",
            Email = "test@test.com",
            Phone = "123-456-7890",
            Address = "123 Main St, Anytown, USA"
        };
        var customerDto = new CustomerDto
        {
            Id = customerDomain.Id,
            FirstName = customerDomain.FirstName,
            LastName = customerDomain.LastName,
            Email = customerDomain.Email,
            Phone = customerDomain.Phone,
            Address = customerDomain.Address
        };

        _customerRepository.Setup(r => r.GetCustomerByIdAsync(customerId)).ReturnsAsync(customerDomain);
        _customerMapper.Setup(m => m.ToDto(customerDomain)).Returns(customerDto);

        // Act
        var result = await _customerService.GetCustomerAsync(customerId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(customerDto.Id, result.Id);
        Assert.Equal(customerDto.FirstName, result.FirstName);
        Assert.Equal(customerDto.LastName, result.LastName);
        Assert.Equal(customerDto.Email, result.Email);
        Assert.Equal(customerDto.Phone, result.Phone);
        Assert.Equal(customerDto.Address, result.Address);
    }

    [Fact]
    public async Task GetCustomerAsync_ShouldThrowAppException_WhenCustomerDoesNotExist()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        _customerRepository.Setup(r => r.GetCustomerByIdAsync(customerId)).ReturnsAsync((Customer?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _customerService.GetCustomerAsync(customerId));
        Assert.Equal($"Customer with id '{customerId}' not found.", exception.Message);
    }
    
    [Fact]
    public async Task CreateCustomerAsync_ShouldThrowAppException_WhenEmailIsNotUnique()
    {
        // Arrange
        var createCustomerDto = new CreateCustomerDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "test@test.com",
            Phone = "123-456-7890",
            Address = "123 Main St, Anytown, USA"
        };
        
        var existingCustomer = new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = "Jane",
            LastName = "Smith",
            Email = "test@test.com",
            Phone = "987-654-3210",
            Address = "456 Another St, Othertown, USA"
        };
        
        _customerRepository.Setup(r => r.GetAllCustomersAsync()).ReturnsAsync(new List<Customer> { existingCustomer });
        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _customerService.CreateCustomerAsync(createCustomerDto));
        Assert.Equal($"A customer with the email '{createCustomerDto.Email}' already exists.", exception.Message);
    }

    [Fact]
    public async Task CreateCustomerAsync_ShouldAllowCreation_WhenEmailIsUnique()
    {
        // Arrange
        var createCustomerDto = new CreateCustomerDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "test@test.com",
            Phone = "123-456-7890",
            Address = "123 Main St, Anytown, USA"
        };

        _customerRepository.Setup(r => r.GetAllCustomersAsync()).ReturnsAsync(new List<Customer>());
        var customerDomain = new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = createCustomerDto.FirstName,
            LastName = createCustomerDto.LastName,
            Email = createCustomerDto.Email,
            Phone = createCustomerDto.Phone,
            Address = createCustomerDto.Address
        };

        var customerDto = new CustomerDto
        {
            Id = customerDomain.Id,
            FirstName = customerDomain.FirstName,
            LastName = customerDomain.LastName,
            Email = customerDomain.Email,
            Phone = customerDomain.Phone,
            Address = customerDomain.Address
        };

        _customerMapper.Setup(m => m.ToDomain(createCustomerDto)).Returns(customerDomain);
        _customerMapper.Setup(m => m.ToDto(customerDomain)).Returns(customerDto);
        _customerRepository.Setup(r => r.CreateCustomerAsync(customerDomain)).ReturnsAsync(customerDomain);
        // Act
        var result = await _customerService.CreateCustomerAsync(createCustomerDto);
        // Assert
        Assert.NotNull(result);
        Assert.Equal(customerDto.Id, result.Id);
        Assert.Equal(customerDto.FirstName, result.FirstName);
        Assert.Equal(customerDto.LastName, result.LastName);
        Assert.Equal(customerDto.Email, result.Email);
        Assert.Equal(customerDto.Phone, result.Phone);
        Assert.Equal(customerDto.Address, result.Address);

        _customerMapper.Verify(m => m.ToDomain(createCustomerDto), Times.Once);
        _customerMapper.Verify(m => m.ToDto(customerDomain), Times.Once);
        _customerRepository.Verify(r => r.GetAllCustomersAsync(), Times.Once);
        _customerRepository.Verify(r => r.CreateCustomerAsync(customerDomain), Times.Once);

    }
    
    [Fact]
    public async Task GetAllCustomersAsync_ShouldReturnAllCustomerDtos()
    {
        // Arrange
        var customerDomains = new List<Customer>
        {
            new Customer
            {
                Id = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Email = "test1@test.com",
                Phone = "123-456-7890",
                Address = "123 Main St, Anytown, USA"
            },
            new Customer
            {
                Id = Guid.NewGuid(),
                FirstName = "Jane",
                LastName = "Smith",
                Email = "test2@test.com",
                Phone = "987-654-3210",
                Address = "456 Another St, Othertown, USA"
                }
        };
        
        var customerDtos = new List<CustomerDto>
        {
            new CustomerDto
            {
                Id = customerDomains[0].Id,
                FirstName = customerDomains[0].FirstName,
                LastName = customerDomains[0].LastName,
                Email = customerDomains[0].Email,
                Phone = customerDomains[0].Phone,
                Address = customerDomains[0].Address
            },
            new CustomerDto
            {
                Id = customerDomains[1].Id,
                FirstName = customerDomains[1].FirstName,
                LastName = customerDomains[1].LastName,
                Email = customerDomains[1].Email,
                Phone = customerDomains[1].Phone,
                Address = customerDomains[1].Address
            }
        };
        
        _customerRepository.Setup(r => r.GetAllCustomersAsync()).ReturnsAsync(customerDomains);
        _customerMapper.Setup(m => m.ToDto(customerDomains[0])).Returns(customerDtos[0]);
        _customerMapper.Setup(m => m.ToDto(customerDomains[1])).Returns(customerDtos[1]);
        
        // Act
        var result = await _customerService.GetAllCustomersAsync(); 
        // Assert
        Assert.NotNull(result);
        var resultList = new List<CustomerDto>(result);
        Assert.Equal(2, resultList.Count);
        Assert.Equal(customerDtos[0].Id, resultList[0].Id);
        Assert.Equal(customerDtos[1].Id, resultList[1].Id);
        
        _customerRepository.Verify(r => r.GetAllCustomersAsync(), Times.Once);
        _customerMapper.Verify(m => m.ToDto(It.IsAny<Customer>()), Times.Exactly(2));
        
    }

    [Fact]
    public async Task UpdateCustomerAsync_ShouldUpdateCustomerSuccessfully_AndReturnDto()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var updateCustomerDto = new UpdateCustomerDto
        {
            FirstName = "UpdatedFirstName",
            LastName = "UpdatedLastName",
            Email = "test@test.com",
            Phone = "123-456-7890",
            Address = "123 Main St, Anytown, USA"
        };

        var customerDomain = new Customer
        {
            Id = customerId,
            FirstName = updateCustomerDto.FirstName,
            LastName = updateCustomerDto.LastName,
            Email = updateCustomerDto.Email,
            Phone = updateCustomerDto.Phone,
            Address = updateCustomerDto.Address
        };

        var customerDto = new CustomerDto
        {
            Id = customerDomain.Id,
            FirstName = customerDomain.FirstName,
            LastName = customerDomain.LastName,
            Email = customerDomain.Email,
            Phone = customerDomain.Phone,
            Address = customerDomain.Address
        };

        _customerMapper.Setup(m => m.ToDomain(updateCustomerDto)).Returns(customerDomain);
        _customerMapper.Setup(m => m.ToDto(customerDomain)).Returns(customerDto);
        _customerRepository.Setup(r => r.UpdateCustomerAsync(customerDomain)).ReturnsAsync(customerDomain);

        // Act
        var result = await _customerService.UpdateCustomerAsync(customerId, updateCustomerDto);
        // Assert
        Assert.NotNull(result);
        Assert.Equal(customerDto.Id, result.Id);
        Assert.Equal(customerDto.FirstName, result.FirstName);
        Assert.Equal(customerDto.LastName, result.LastName);
        Assert.Equal(customerDto.Email, result.Email);
        Assert.Equal(customerDto.Phone, result.Phone);
        Assert.Equal(customerDto.Address, result.Address);

        _customerMapper.Verify(m => m.ToDomain(updateCustomerDto), Times.Once);
        _customerMapper.Verify(m => m.ToDto(customerDomain), Times.Once);
        _customerRepository.Verify(r => r.UpdateCustomerAsync(customerDomain), Times.Once);

    }
    
    [Fact]
    public async Task UpdateCustomerAsync_ShouldThrowAppException_WhenCustomerDoesNotExist()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var updateCustomerDto = new UpdateCustomerDto
        {
            FirstName = "UpdatedFirstName",
            LastName = "UpdatedLastName",
            Email = "test@test.com",
            Phone = "123-456-7890",
            Address = "123 Main St, Anytown, USA"
        };  
        
        var customerDomain = new Customer
        {
            Id = customerId,
            FirstName = updateCustomerDto.FirstName,
            LastName = updateCustomerDto.LastName,
            Email = updateCustomerDto.Email,
            Phone = updateCustomerDto.Phone,
            Address = updateCustomerDto.Address
        };
        
        _customerMapper.Setup(m => m.ToDomain(updateCustomerDto)).Returns(customerDomain);
        _customerRepository.Setup(r => r.UpdateCustomerAsync(customerDomain)).ReturnsAsync((Customer?)null);
        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _customerService.UpdateCustomerAsync(customerId, updateCustomerDto));
        Assert.Equal($"Customer with id '{customerId}' not found.", exception.Message);
    }

    [Fact]
    public async Task DeleteCustomerAsync_ShouldDeleteCustomerSuccessfully_AndReturnDto()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customerDomain = new Customer
        {
            Id = customerId,
            FirstName = "John",
            LastName = "Doe",
            Email = "test@test.com",
            Phone = "123-456-7890",
            Address = "123 Main St, Anytown, USA"
        };

        var customerDto = new CustomerDto
        {
            Id = customerDomain.Id,
            FirstName = customerDomain.FirstName,
            LastName = customerDomain.LastName,
            Email = customerDomain.Email,
            Phone = customerDomain.Phone,
            Address = customerDomain.Address
        };

        _customerRepository.Setup(r => r.DeleteCustomerByIdAsync(customerId)).ReturnsAsync(customerDomain);
        _customerMapper.Setup(m => m.ToDto(customerDomain)).Returns(customerDto);
        // Act
        var result = await _customerService.DeleteCustomerAsync(customerId);
        // Assert
        Assert.NotNull(result);
        Assert.Equal(customerDto.Id, result.Id);
        Assert.Equal(customerDto.FirstName, result.FirstName);
        Assert.Equal(customerDto.LastName, result.LastName);
        Assert.Equal(customerDto.Email, result.Email);
        Assert.Equal(customerDto.Phone, result.Phone);
        Assert.Equal(customerDto.Address, result.Address);

        _customerRepository.Verify(r => r.DeleteCustomerByIdAsync(customerId), Times.Once);
        _customerMapper.Verify(m => m.ToDto(customerDomain), Times.Once);
    }

    [Fact]
    public async Task DeleteCustomerAsync_ShouldThrowAppException_WhenCustomerDoesNotExist()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        _customerRepository.Setup(r => r.DeleteCustomerByIdAsync(customerId)).ReturnsAsync((Customer?)null);
        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _customerService.DeleteCustomerAsync(customerId));
        Assert.Equal($"Customer with id '{customerId}' not found.", exception.Message);
    }

}
