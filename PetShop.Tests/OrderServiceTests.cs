using FluentAssertions;
using Moq;
using PetShop.Application.DTOs;
using PetShop.Application.Repositories;
using PetShop.Application.Services;
using PetShop.Domain;

namespace PetShop.Tests;

/// <summary>
/// Integration tests for the OrderService application service.
/// Tests use mocked repositories to verify service orchestration, state transitions, and cost calculations.
/// </summary>
public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _orderService = new OrderService(
            _orderRepositoryMock.Object,
            _customerRepositoryMock.Object);
    }

    #region CreateOrderAsync Tests

    [Fact]
    public async Task CreateOrderAsync_WithValidRequest_ShouldCreateOrderAndReturnDto()
    {
        // Arrange
        var customer = new Customer("John", "Doe");
        var customerId = customer.Id;
        var pickupDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

        var request = new CreateOrderRequest
        {
            CustomerId = customerId,
            PickupDate = pickupDate
        };

        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync(customer);

        Order? capturedOrder = null;
        _orderRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Order>()))
            .Returns<Order>(async order =>
            {
                capturedOrder = order;
                return await Task.FromResult(order);
            });

        // Act
        var result = await _orderService.CreateOrderAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.CustomerId.Should().Be(customerId);
        result.PickupDate.Should().Be(pickupDate);
        result.Status.Should().Be(OrderStatus.Open);
        result.Pets.Should().NotBeNull().And.BeEmpty();
        result.EstimatedCost.Should().Be(0m);
        result.ActualCost.Should().BeNull();

        capturedOrder.Should().NotBeNull();
        _orderRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Order>()), Times.Once);
    }

    [Fact]
    public async Task CreateOrderAsync_WithNonExistentCustomer_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var request = new CreateOrderRequest
        {
            CustomerId = customerId,
            PickupDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1))
        };

        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync((Customer?)null);

        // Act
        var act = async () => await _orderService.CreateOrderAsync(request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Customer with ID {customerId} was not found.");

        _orderRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task CreateOrderAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Arrange
        CreateOrderRequest? request = null;

        // Act
        var act = async () => await _orderService.CreateOrderAsync(request!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("request");
    }

    [Fact]
    public async Task CreateOrderAsync_WithPastPickupDate_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        var customer = new Customer("John", "Doe");
        var customerId = customer.Id;
        var pastDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));

        var request = new CreateOrderRequest
        {
            CustomerId = customerId,
            PickupDate = pastDate
        };

        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync(customer);

        // Act
        var act = async () => await _orderService.CreateOrderAsync(request);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleViolationException>();
    }

    #endregion

    #region GetOrderByIdAsync Tests

    [Fact]
    public async Task GetOrderByIdAsync_WithExistingOrder_ShouldReturnOrderDto()
    {
        // Arrange
        var customer = new Customer("John", "Doe");
        var customerId = customer.Id;
        var order = new Order(customerId, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var orderId = order.Id;

        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        // Act
        var result = await _orderService.GetOrderByIdAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(orderId);
        result.CustomerId.Should().Be(customerId);
        result.Status.Should().Be(OrderStatus.Open);
        result.EstimatedCost.Should().Be(0m);
        result.ActualCost.Should().BeNull();
    }

    [Fact]
    public async Task GetOrderByIdAsync_WithOrderContainingPets_ShouldCalculateEstimatedCost()
    {
        // Arrange
        var customer = new Customer("John", "Doe");
        var customerId = customer.Id;
        var order = new Order(customerId, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var orderId = order.Id;

        var pet1 = new Pet(orderId, "Fluffy", 100m);
        var pet2 = new Pet(orderId, "Spot", 150m);
        order.AddPet(pet1);
        order.AddPet(pet2);

        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        // Act
        var result = await _orderService.GetOrderByIdAsync(orderId);

        // Assert
        result.EstimatedCost.Should().Be(250m); // 100 + 150
        result.ActualCost.Should().BeNull();
        result.Pets.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetOrderByIdAsync_WithDeliveredOrder_ShouldReturnActualCost()
    {
        // Arrange
        var customer = new Customer("John", "Doe");
        var customerId = customer.Id;
        var order = new Order(customerId, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var orderId = order.Id;

        var pet = new Pet(orderId, "Fluffy", 100m);
        order.AddPet(pet);
        order.TransitionToProcessing();
        order.TransitionToDelivered();

        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        // Act
        var result = await _orderService.GetOrderByIdAsync(orderId);

        // Assert
        result.Status.Should().Be(OrderStatus.Delivered);
        result.EstimatedCost.Should().BeNull();
        result.ActualCost.Should().Be(100m);
    }

    [Fact]
    public async Task GetOrderByIdAsync_WithNonExistentOrder_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync((Order?)null);

        // Act
        var act = async () => await _orderService.GetOrderByIdAsync(orderId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Order with ID {orderId} was not found.");
    }

    #endregion

    #region UpdateOrderAsync Tests

    [Fact]
    public async Task UpdateOrderAsync_WithValidRequest_ShouldUpdatePickupDate()
    {
        // Arrange
        var customer = new Customer("John", "Doe");
        var customerId = customer.Id;
        var order = new Order(customerId, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var orderId = order.Id;
        var newPickupDate = DateOnly.FromDateTime(DateTime.Today.AddDays(2));

        var request = new UpdateOrderRequest
        {
            PickupDate = newPickupDate
        };

        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        _orderRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Order>()))
            .ReturnsAsync(order);

        // Act
        var result = await _orderService.UpdateOrderAsync(orderId, request);

        // Assert
        result.PickupDate.Should().Be(newPickupDate);
        _orderRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Order>()), Times.Once);
    }

    [Fact]
    public async Task UpdateOrderAsync_WithDeliveredOrder_ShouldThrowInvalidOrderStateException()
    {
        // Arrange
        var customer = new Customer("John", "Doe");
        var customerId = customer.Id;
        var order = new Order(customerId, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var orderId = order.Id;

        var pet = new Pet(orderId, "Fluffy", 100m);
        order.AddPet(pet);
        order.TransitionToProcessing();
        order.TransitionToDelivered();

        var request = new UpdateOrderRequest
        {
            PickupDate = DateOnly.FromDateTime(DateTime.Today.AddDays(2))
        };

        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        // Act
        var act = async () => await _orderService.UpdateOrderAsync(orderId, request);

        // Assert
        await act.Should().ThrowAsync<InvalidOrderStateException>();

        _orderRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task UpdateOrderAsync_WithNonExistentOrder_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var request = new UpdateOrderRequest
        {
            PickupDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1))
        };

        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync((Order?)null);

        // Act
        var act = async () => await _orderService.UpdateOrderAsync(orderId, request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Order with ID {orderId} was not found.");
    }

    [Fact]
    public async Task UpdateOrderAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        UpdateOrderRequest? request = null;

        // Act
        var act = async () => await _orderService.UpdateOrderAsync(orderId, request!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("request");
    }

    #endregion

    #region TransitionOrderToProcessingAsync Tests

    [Fact]
    public async Task TransitionOrderToProcessingAsync_WithOpenOrder_ShouldTransitionToProcessing()
    {
        // Arrange
        var customer = new Customer("John", "Doe");
        var customerId = customer.Id;
        var order = new Order(customerId, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var orderId = order.Id;

        var pet = new Pet(orderId, "Fluffy", 100m);
        order.AddPet(pet);

        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        _orderRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Order>()))
            .ReturnsAsync(order);

        // Act
        var result = await _orderService.TransitionOrderToProcessingAsync(orderId);

        // Assert
        result.Status.Should().Be(OrderStatus.Processing);
        _orderRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Order>()), Times.Once);
    }

    [Fact]
    public async Task TransitionOrderToProcessingAsync_WithOrderWithoutPets_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        var customer = new Customer("John", "Doe");
        var customerId = customer.Id;
        var order = new Order(customerId, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var orderId = order.Id;

        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        // Act
        var act = async () => await _orderService.TransitionOrderToProcessingAsync(orderId);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleViolationException>()
            .WithMessage("Order must have at least one pet before transitioning to Processing.");

        _orderRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task TransitionOrderToProcessingAsync_WithProcessingOrder_ShouldThrowInvalidOrderStateException()
    {
        // Arrange
        var customer = new Customer("John", "Doe");
        var customerId = customer.Id;
        var order = new Order(customerId, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var orderId = order.Id;

        var pet = new Pet(orderId, "Fluffy", 100m);
        order.AddPet(pet);
        order.TransitionToProcessing();

        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        // Act
        var act = async () => await _orderService.TransitionOrderToProcessingAsync(orderId);

        // Assert
        await act.Should().ThrowAsync<InvalidOrderStateException>();
    }

    [Fact]
    public async Task TransitionOrderToProcessingAsync_WithNonExistentOrder_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync((Order?)null);

        // Act
        var act = async () => await _orderService.TransitionOrderToProcessingAsync(orderId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Order with ID {orderId} was not found.");
    }

    #endregion

    #region TransitionOrderToDeliveredAsync Tests

    [Fact]
    public async Task TransitionOrderToDeliveredAsync_WithProcessingOrder_ShouldTransitionToDelivered()
    {
        // Arrange
        var customer = new Customer("John", "Doe");
        var customerId = customer.Id;
        var order = new Order(customerId, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var orderId = order.Id;

        var pet = new Pet(orderId, "Fluffy", 100m);
        order.AddPet(pet);
        order.TransitionToProcessing();

        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        _orderRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Order>()))
            .ReturnsAsync(order);

        // Act
        var result = await _orderService.TransitionOrderToDeliveredAsync(orderId);

        // Assert
        result.Status.Should().Be(OrderStatus.Delivered);
        result.ActualCost.Should().Be(100m);
        _orderRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Order>()), Times.Once);
    }

    [Fact]
    public async Task TransitionOrderToDeliveredAsync_WithOpenOrder_ShouldThrowInvalidOrderStateException()
    {
        // Arrange
        var customer = new Customer("John", "Doe");
        var customerId = customer.Id;
        var order = new Order(customerId, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var orderId = order.Id;

        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        // Act
        var act = async () => await _orderService.TransitionOrderToDeliveredAsync(orderId);

        // Assert
        await act.Should().ThrowAsync<InvalidOrderStateException>();
    }

    [Fact]
    public async Task TransitionOrderToDeliveredAsync_WithNonExistentOrder_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync((Order?)null);

        // Act
        var act = async () => await _orderService.TransitionOrderToDeliveredAsync(orderId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Order with ID {orderId} was not found.");
    }

    #endregion

    #region AddPetToOrderAsync Tests

    [Fact]
    public async Task AddPetToOrderAsync_WithOpenOrder_ShouldAddPet()
    {
        // Arrange
        var customer = new Customer("John", "Doe");
        var customerId = customer.Id;
        var order = new Order(customerId, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var orderId = order.Id;

        var request = new AddPetRequest
        {
            Name = "Fluffy",
            Price = 100m,
            Kind = "Dog",
            Color = "Brown"
        };

        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        _orderRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Order>()))
            .ReturnsAsync(order);

        // Act
        var result = await _orderService.AddPetToOrderAsync(orderId, request);

        // Assert
        result.Pets.Should().HaveCount(1);
        result.Pets.First().Name.Should().Be("Fluffy");
        result.Pets.First().Price.Should().Be(100m);
        result.Pets.First().Kind.Should().Be("Dog");
        result.Pets.First().Color.Should().Be("Brown");
        _orderRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Order>()), Times.Once);
    }

    [Fact]
    public async Task AddPetToOrderAsync_WithProcessingOrder_ShouldThrowInvalidOrderStateException()
    {
        // Arrange
        var customer = new Customer("John", "Doe");
        var customerId = customer.Id;
        var order = new Order(customerId, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var orderId = order.Id;

        var pet = new Pet(orderId, "Fluffy", 100m);
        order.AddPet(pet);
        order.TransitionToProcessing();

        var request = new AddPetRequest
        {
            Name = "Spot",
            Price = 150m
        };

        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        // Act
        var act = async () => await _orderService.AddPetToOrderAsync(orderId, request);

        // Assert
        await act.Should().ThrowAsync<InvalidOrderStateException>();

        _orderRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task AddPetToOrderAsync_WithNonExistentOrder_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var request = new AddPetRequest
        {
            Name = "Fluffy",
            Price = 100m
        };

        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync((Order?)null);

        // Act
        var act = async () => await _orderService.AddPetToOrderAsync(orderId, request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Order with ID {orderId} was not found.");
    }

    [Fact]
    public async Task AddPetToOrderAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        AddPetRequest? request = null;

        // Act
        var act = async () => await _orderService.AddPetToOrderAsync(orderId, request!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("request");
    }

    [Fact]
    public async Task AddPetToOrderAsync_WithInvalidPrice_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        var customer = new Customer("John", "Doe");
        var customerId = customer.Id;
        var order = new Order(customerId, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var orderId = order.Id;

        var request = new AddPetRequest
        {
            Name = "Fluffy",
            Price = 0m
        };

        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        // Act
        var act = async () => await _orderService.AddPetToOrderAsync(orderId, request);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleViolationException>();
    }

    #endregion

    #region RemovePetFromOrderAsync Tests

    [Fact]
    public async Task RemovePetFromOrderAsync_WithOpenOrder_ShouldRemovePet()
    {
        // Arrange
        var customer = new Customer("John", "Doe");
        var customerId = customer.Id;
        var order = new Order(customerId, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var orderId = order.Id;

        var pet = new Pet(orderId, "Fluffy", 100m);
        order.AddPet(pet);
        var petId = pet.Id;

        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        _orderRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Order>()))
            .ReturnsAsync(order);

        // Act
        var result = await _orderService.RemovePetFromOrderAsync(orderId, petId);

        // Assert
        result.Pets.Should().BeEmpty();
        _orderRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Order>()), Times.Once);
    }

    [Fact]
    public async Task RemovePetFromOrderAsync_WithProcessingOrder_ShouldThrowInvalidOrderStateException()
    {
        // Arrange
        var customer = new Customer("John", "Doe");
        var customerId = customer.Id;
        var order = new Order(customerId, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var orderId = order.Id;

        var pet = new Pet(orderId, "Fluffy", 100m);
        order.AddPet(pet);
        order.TransitionToProcessing();
        var petId = pet.Id;

        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        // Act
        var act = async () => await _orderService.RemovePetFromOrderAsync(orderId, petId);

        // Assert
        await act.Should().ThrowAsync<InvalidOrderStateException>();

        _orderRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task RemovePetFromOrderAsync_WithNonExistentOrder_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var petId = Guid.NewGuid();

        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync((Order?)null);

        // Act
        var act = async () => await _orderService.RemovePetFromOrderAsync(orderId, petId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Order with ID {orderId} was not found.");
    }

    [Fact]
    public async Task RemovePetFromOrderAsync_WithNonExistentPet_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var customer = new Customer("John", "Doe");
        var customerId = customer.Id;
        var order = new Order(customerId, DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var orderId = order.Id;
        var petId = Guid.NewGuid();

        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        // Act
        var act = async () => await _orderService.RemovePetFromOrderAsync(orderId, petId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Pet with ID {petId} was not found in order {orderId}.");
    }

    #endregion
}
