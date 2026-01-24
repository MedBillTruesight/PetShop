using FluentAssertions;
using Moq;
using PetShop.Application.DTOs;
using PetShop.Application.Repositories;
using PetShop.Application.Services;
using PetShop.Domain;

namespace PetShop.Tests;

/// <summary>
/// Integration tests for the CustomerService application service.
/// Tests use mocked repositories to verify service orchestration and payment calculations.
/// </summary>
public class CustomerServiceTests
{
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly CustomerService _customerService;

    public CustomerServiceTests()
    {
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _customerService = new CustomerService(
            _customerRepositoryMock.Object,
            _orderRepositoryMock.Object);
    }

    #region CreateCustomerAsync Tests

    [Fact]
    public async Task CreateCustomerAsync_WithValidRequest_ShouldCreateCustomerAndReturnDto()
    {
        // Arrange
        var request = new CreateCustomerRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Phone = "555-1234"
        };

        Customer? capturedCustomer = null;
        _customerRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Customer>()))
            .Returns<Customer>(async customer =>
            {
                capturedCustomer = customer;
                return await Task.FromResult(customer);
            });

        _orderRepositoryMock
            .Setup(r => r.GetByCustomerIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(Enumerable.Empty<Order>());

        // Act
        var result = await _customerService.CreateCustomerAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be(request.FirstName);
        result.LastName.Should().Be(request.LastName);
        result.Email.Should().Be(request.Email);
        result.Phone.Should().Be(request.Phone);
        result.Id.Should().NotBeEmpty();
        result.EstimatedPaymentDue.Should().Be(0m);
        result.ActualPaymentDue.Should().Be(0m);

        capturedCustomer.Should().NotBeNull();
        _customerRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Customer>()), Times.Once);
    }

    [Fact]
    public async Task CreateCustomerAsync_WithMinimalData_ShouldCreateCustomer()
    {
        // Arrange
        var request = new CreateCustomerRequest
        {
            FirstName = "Jane",
            LastName = "Smith"
        };

        Customer? capturedCustomer = null;
        _customerRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Customer>()))
            .Returns<Customer>(async customer =>
            {
                capturedCustomer = customer;
                return await Task.FromResult(customer);
            });

        _orderRepositoryMock
            .Setup(r => r.GetByCustomerIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(Enumerable.Empty<Order>());

        // Act
        var result = await _customerService.CreateCustomerAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be(request.FirstName);
        result.LastName.Should().Be(request.LastName);
        result.Email.Should().BeNull();
        result.Phone.Should().BeNull();
    }

    [Fact]
    public async Task CreateCustomerAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Arrange
        CreateCustomerRequest? request = null;

        // Act
        var act = async () => await _customerService.CreateCustomerAsync(request!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("request");
    }

    [Fact]
    public async Task CreateCustomerAsync_WithInvalidEmail_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        var request = new CreateCustomerRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "invalid-email"
        };

        _customerRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Customer>()))
            .ThrowsAsync(new BusinessRuleViolationException("Invalid email format: invalid-email"));

        // Act
        var act = async () => await _customerService.CreateCustomerAsync(request);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleViolationException>();
    }

    #endregion

    #region GetCustomerByIdAsync Tests

    [Fact]
    public async Task GetCustomerByIdAsync_WithExistingCustomer_ShouldReturnCustomerDto()
    {
        // Arrange
        var customer = new Customer("John", "Doe", "john@example.com", "555-1234");
        var customerId = customer.Id;

        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync(customer);

        _orderRepositoryMock
            .Setup(r => r.GetByCustomerIdAsync(customerId))
            .ReturnsAsync(Enumerable.Empty<Order>());

        // Act
        var result = await _customerService.GetCustomerByIdAsync(customerId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(customerId);
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Email.Should().Be("john@example.com");
        result.Phone.Should().Be("555-1234");
        result.EstimatedPaymentDue.Should().Be(0m);
        result.ActualPaymentDue.Should().Be(0m);
    }

    [Fact]
    public async Task GetCustomerByIdAsync_WithNonExistentCustomer_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync((Customer?)null);

        // Act
        var act = async () => await _customerService.GetCustomerByIdAsync(customerId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Customer with ID {customerId} was not found.");
    }

    [Fact]
    public async Task GetCustomerByIdAsync_WithOrders_ShouldCalculateEstimatedPaymentDue()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = new Customer("John", "Doe");

        var order1 = new Order(customerId, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var pet1 = new Pet(order1.Id, "Fluffy", 100m);
        order1.AddPet(pet1);

        var order2 = new Order(customerId, DateOnly.FromDateTime(DateTime.Today.AddDays(2)));
        var pet2 = new Pet(order2.Id, "Spot", 150m);
        order2.AddPet(pet2);
        order2.TransitionToProcessing();

        var orders = new[] { order1, order2 };

        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync(customer);

        _orderRepositoryMock
            .Setup(r => r.GetByCustomerIdAsync(customerId))
            .ReturnsAsync(orders);

        // Act
        var result = await _customerService.GetCustomerByIdAsync(customerId);

        // Assert
        result.EstimatedPaymentDue.Should().Be(250m); // 100 + 150
        result.ActualPaymentDue.Should().Be(0m);
    }

    [Fact]
    public async Task GetCustomerByIdAsync_WithDeliveredOrders_ShouldCalculateActualPaymentDue()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = new Customer("John", "Doe");

        var order1 = new Order(customerId, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var pet1 = new Pet(order1.Id, "Fluffy", 100m);
        order1.AddPet(pet1);
        order1.TransitionToProcessing();
        order1.TransitionToDelivered();

        var order2 = new Order(customerId, DateOnly.FromDateTime(DateTime.Today.AddDays(2)));
        var pet2 = new Pet(order2.Id, "Spot", 150m);
        order2.AddPet(pet2);
        order2.TransitionToProcessing();
        order2.TransitionToDelivered();

        var orders = new[] { order1, order2 };

        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync(customer);

        _orderRepositoryMock
            .Setup(r => r.GetByCustomerIdAsync(customerId))
            .ReturnsAsync(orders);

        // Act
        var result = await _customerService.GetCustomerByIdAsync(customerId);

        // Assert
        result.EstimatedPaymentDue.Should().Be(0m);
        result.ActualPaymentDue.Should().Be(250m); // 100 + 150
    }

    [Fact]
    public async Task GetCustomerByIdAsync_WithMixedOrderStatuses_ShouldCalculateBothPaymentDues()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = new Customer("John", "Doe");

        // Open order
        var openOrder = new Order(customerId, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var pet1 = new Pet(openOrder.Id, "Fluffy", 100m);
        openOrder.AddPet(pet1);

        // Processing order
        var processingOrder = new Order(customerId, DateOnly.FromDateTime(DateTime.Today.AddDays(2)));
        var pet2 = new Pet(processingOrder.Id, "Spot", 150m);
        processingOrder.AddPet(pet2);
        processingOrder.TransitionToProcessing();

        // Delivered order
        var deliveredOrder = new Order(customerId, DateOnly.FromDateTime(DateTime.Today.AddDays(3)));
        var pet3 = new Pet(deliveredOrder.Id, "Max", 200m);
        deliveredOrder.AddPet(pet3);
        deliveredOrder.TransitionToProcessing();
        deliveredOrder.TransitionToDelivered();

        var orders = new[] { openOrder, processingOrder, deliveredOrder };

        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync(customer);

        _orderRepositoryMock
            .Setup(r => r.GetByCustomerIdAsync(customerId))
            .ReturnsAsync(orders);

        // Act
        var result = await _customerService.GetCustomerByIdAsync(customerId);

        // Assert
        result.EstimatedPaymentDue.Should().Be(250m); // 100 (Open) + 150 (Processing)
        result.ActualPaymentDue.Should().Be(200m); // 200 (Delivered)
    }

    #endregion

    #region UpdateCustomerAsync Tests

    [Fact]
    public async Task UpdateCustomerAsync_WithValidRequest_ShouldUpdateCustomerAndReturnDto()
    {
        // Arrange
        var existingCustomer = new Customer("John", "Doe", "john@example.com", "555-1234");
        var customerId = existingCustomer.Id;

        var request = new UpdateCustomerRequest
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane@example.com",
            Phone = "555-5678"
        };

        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync(existingCustomer);

        _customerRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Customer>()))
            .ReturnsAsync(existingCustomer);

        _orderRepositoryMock
            .Setup(r => r.GetByCustomerIdAsync(customerId))
            .ReturnsAsync(Enumerable.Empty<Order>());

        // Act
        var result = await _customerService.UpdateCustomerAsync(customerId, request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(customerId);
        result.FirstName.Should().Be("Jane");
        result.LastName.Should().Be("Smith");
        result.Email.Should().Be("jane@example.com");
        result.Phone.Should().Be("555-5678");

        _customerRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Customer>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCustomerAsync_WithNonExistentCustomer_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var request = new UpdateCustomerRequest
        {
            FirstName = "Jane",
            LastName = "Smith"
        };

        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync((Customer?)null);

        // Act
        var act = async () => await _customerService.UpdateCustomerAsync(customerId, request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Customer with ID {customerId} was not found.");

        _customerRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Customer>()), Times.Never);
    }

    [Fact]
    public async Task UpdateCustomerAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        UpdateCustomerRequest? request = null;

        // Act
        var act = async () => await _customerService.UpdateCustomerAsync(customerId, request!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("request");
    }

    [Fact]
    public async Task UpdateCustomerAsync_WithInvalidEmail_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var existingCustomer = new Customer("John", "Doe");

        var request = new UpdateCustomerRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "invalid-email"
        };

        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync(existingCustomer);

        // Act
        var act = async () => await _customerService.UpdateCustomerAsync(customerId, request);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleViolationException>();
    }

    [Fact]
    public async Task UpdateCustomerAsync_WithOrders_ShouldCalculatePaymentDues()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = new Customer("John", "Doe");

        var order = new Order(customerId, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var pet = new Pet(order.Id, "Fluffy", 100m);
        order.AddPet(pet);
        order.TransitionToProcessing();
        order.TransitionToDelivered();

        var request = new UpdateCustomerRequest
        {
            FirstName = "John",
            LastName = "Doe"
        };

        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync(customer);

        _customerRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Customer>()))
            .ReturnsAsync(customer);

        _orderRepositoryMock
            .Setup(r => r.GetByCustomerIdAsync(customerId))
            .ReturnsAsync(new[] { order });

        // Act
        var result = await _customerService.UpdateCustomerAsync(customerId, request);

        // Assert
        result.EstimatedPaymentDue.Should().Be(0m);
        result.ActualPaymentDue.Should().Be(100m);
    }

    #endregion

    #region GetAllCustomersAsync Tests

    [Fact]
    public async Task GetAllCustomersAsync_WithMultipleCustomers_ShouldReturnAllCustomersWithPaymentCalculations()
    {
        // Arrange
        var customer1 = new Customer("John", "Doe", "john@example.com", "555-1234");
        var customer2 = new Customer("Jane", "Smith", "jane@example.com", "555-5678");

        var customers = new[] { customer1, customer2 };

        // Create orders for customer1
        var order1 = new Order(customer1.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var pet1 = new Pet(order1.Id, "Fluffy", 100m);
        order1.AddPet(pet1);

        var order2 = new Order(customer1.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(2)));
        var pet2 = new Pet(order2.Id, "Spot", 150m);
        order2.AddPet(pet2);
        order2.TransitionToProcessing();

        var orders = new[] { order1, order2 };

        _customerRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(customers);

        _orderRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(orders);

        // Act
        var result = await _customerService.GetAllCustomersAsync();

        // Assert
        result.Should().HaveCount(2);

        var resultList = result.ToList();
        var johnDto = resultList.First(c => c.Id == customer1.Id);
        var janeDto = resultList.First(c => c.Id == customer2.Id);

        johnDto.FirstName.Should().Be("John");
        johnDto.LastName.Should().Be("Doe");
        johnDto.EstimatedPaymentDue.Should().Be(250m); // 100 + 150
        johnDto.ActualPaymentDue.Should().Be(0m);

        janeDto.FirstName.Should().Be("Jane");
        janeDto.LastName.Should().Be("Smith");
        janeDto.EstimatedPaymentDue.Should().Be(0m);
        janeDto.ActualPaymentDue.Should().Be(0m);
    }

    [Fact]
    public async Task GetAllCustomersAsync_WithNoCustomers_ShouldReturnEmptyCollection()
    {
        // Arrange
        _customerRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(Array.Empty<Customer>());

        _orderRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(Array.Empty<Order>());

        // Act
        var result = await _customerService.GetAllCustomersAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllCustomersAsync_WithDeliveredOrders_ShouldCalculateActualPaymentDue()
    {
        // Arrange
        var customer = new Customer("John", "Doe");

        var order = new Order(customer.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var pet = new Pet(order.Id, "Fluffy", 100m);
        order.AddPet(pet);
        order.TransitionToProcessing();
        order.TransitionToDelivered();

        var customers = new[] { customer };
        var orders = new[] { order };

        _customerRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(customers);

        _orderRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(orders);

        // Act
        var result = await _customerService.GetAllCustomersAsync();

        // Assert
        result.Should().HaveCount(1);
        var customerDto = result.First();
        customerDto.EstimatedPaymentDue.Should().Be(0m);
        customerDto.ActualPaymentDue.Should().Be(100m);
    }

    #endregion

    #region GetCustomerOrdersAsync Tests

    [Fact]
    public async Task GetCustomerOrdersAsync_WithExistingCustomerAndOrders_ShouldReturnOrderDtos()
    {
        // Arrange
        var customer = new Customer("John", "Doe");
        var customerId = customer.Id;

        var order1 = new Order(customerId, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var pet1 = new Pet(order1.Id, "Fluffy", 100m);
        order1.AddPet(pet1);

        var order2 = new Order(customerId, DateOnly.FromDateTime(DateTime.Today.AddDays(2)));
        var pet2 = new Pet(order2.Id, "Spot", 150m);
        order2.AddPet(pet2);
        order2.TransitionToProcessing();

        var orders = new[] { order1, order2 };

        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync(customer);

        _orderRepositoryMock
            .Setup(r => r.GetByCustomerIdAsync(customerId))
            .ReturnsAsync(orders);

        // Act
        var result = await _customerService.GetCustomerOrdersAsync(customerId);

        // Assert
        result.Should().HaveCount(2);

        var resultList = result.ToList();
        var order1Dto = resultList.First(o => o.Id == order1.Id);
        var order2Dto = resultList.First(o => o.Id == order2.Id);

        order1Dto.Status.Should().Be(OrderStatus.Open);
        order1Dto.EstimatedCost.Should().Be(100m);
        order1Dto.ActualCost.Should().BeNull();
        order1Dto.Pets.Should().HaveCount(1);
        order1Dto.Pets.First().Name.Should().Be("Fluffy");

        order2Dto.Status.Should().Be(OrderStatus.Processing);
        order2Dto.EstimatedCost.Should().Be(150m);
        order2Dto.ActualCost.Should().BeNull();
        order2Dto.Pets.Should().HaveCount(1);
        order2Dto.Pets.First().Name.Should().Be("Spot");
    }

    [Fact]
    public async Task GetCustomerOrdersAsync_WithExistingCustomerNoOrders_ShouldReturnEmptyCollection()
    {
        // Arrange
        var customer = new Customer("John", "Doe");
        var customerId = customer.Id;

        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync(customer);

        _orderRepositoryMock
            .Setup(r => r.GetByCustomerIdAsync(customerId))
            .ReturnsAsync(Array.Empty<Order>());

        // Act
        var result = await _customerService.GetCustomerOrdersAsync(customerId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCustomerOrdersAsync_WithNonExistentCustomer_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync((Customer?)null);

        // Act
        var act = async () => await _customerService.GetCustomerOrdersAsync(customerId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Customer with ID {customerId} was not found.");
    }

    [Fact]
    public async Task GetCustomerOrdersAsync_WithDeliveredOrder_ShouldIncludeActualCost()
    {
        // Arrange
        var customer = new Customer("John", "Doe");
        var customerId = customer.Id;

        var order = new Order(customerId, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var pet = new Pet(order.Id, "Fluffy", 100m);
        order.AddPet(pet);
        order.TransitionToProcessing();
        order.TransitionToDelivered();

        var orders = new[] { order };

        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync(customer);

        _orderRepositoryMock
            .Setup(r => r.GetByCustomerIdAsync(customerId))
            .ReturnsAsync(orders);

        // Act
        var result = await _customerService.GetCustomerOrdersAsync(customerId);

        // Assert
        result.Should().HaveCount(1);
        var orderDto = result.First();
        orderDto.Status.Should().Be(OrderStatus.Delivered);
        orderDto.ActualCost.Should().Be(100m);
        orderDto.EstimatedCost.Should().BeNull();
    }

    #endregion

    #region DeleteCustomerAsync Tests

    [Fact]
    public async Task DeleteCustomerAsync_WithExistingCustomerNoOrders_ShouldDeleteSuccessfully()
    {
        // Arrange
        var customer = new Customer("John", "Doe");
        var customerId = customer.Id;

        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync(customer);

        _customerRepositoryMock
            .Setup(r => r.DeleteAsync(customerId))
            .Returns(Task.CompletedTask);

        // Act
        await _customerService.DeleteCustomerAsync(customerId);

        // Assert
        _customerRepositoryMock.Verify(r => r.DeleteAsync(customerId), Times.Once);
    }

    [Fact]
    public async Task DeleteCustomerAsync_WithNonExistentCustomer_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync((Customer?)null);

        // Act
        var act = async () => await _customerService.DeleteCustomerAsync(customerId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Customer with ID {customerId} was not found.");

        _customerRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task DeleteCustomerAsync_WithCustomerHavingOrders_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        var customer = new Customer("John", "Doe");
        var customerId = customer.Id;

        // Create an order for this customer
        var order = new Order(customerId, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var pet = new Pet(order.Id, "Fluffy", 100m);
        order.AddPet(pet);

        var orders = new[] { order };

        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync(customer);

        _orderRepositoryMock
            .Setup(r => r.GetByCustomerIdAsync(customerId))
            .ReturnsAsync(orders);

        // Act
        var act = async () => await _customerService.DeleteCustomerAsync(customerId);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleViolationException>()
            .WithMessage("Cannot delete customer 'John Doe' because they have 1 order(s).");

        _customerRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    #endregion
}
