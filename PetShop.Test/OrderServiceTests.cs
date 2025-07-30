using Moq;
using PetShop.Application.DTOs;
using PetShop.Application.Features.Customer.Interfaces;
using PetShop.Application.Features.Order.Interfaces;
using PetShop.Application.Features.Order.Services;
using PetShop.Application.Features.Pet.Interfaces;
using PetShop.Application.Mappers;
using PetShop.Domain.Entities;
using PetShop.Domain.Enums;
using PetShop.Domain.Interfaces;
using Xunit;

namespace PetShop.Test;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _mockRepository;
    private readonly Mock<IOrderMapper> _mockMapper;
    private readonly Mock<ICustomerService> _mockCustomerService;
    private readonly Mock<IPetService> _mockPetService;
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        _mockRepository = new Mock<IOrderRepository>();
        _mockMapper = new Mock<IOrderMapper>();
        _mockCustomerService = new Mock<ICustomerService>();
        _mockPetService = new Mock<IPetService>();
        _orderService = new OrderService(_mockRepository.Object, _mockMapper.Object, _mockCustomerService.Object, _mockPetService.Object);
    }

    [Fact]
    public async Task CreateOrderAsync_ValidData_ReturnsOrderDto()
    {
        // Arrange
        var createOrderDto = new CreateOrderDto
        {
            CustomerId = 1,
            PickupDate = DateTime.UtcNow.AddDays(1)
        };

        var customer = new CustomerDto
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        var order = new Order
        {
            Id = 1,
            CustomerId = 1,
            PickupDate = DateTime.UtcNow.AddDays(1),
            Status = OrderStatus.Open
        };

        var expectedOrderDto = new OrderDto
        {
            Id = 1,
            CustomerId = 1,
            CustomerName = "John Doe",
            PickupDate = DateTime.UtcNow.AddDays(1),
            Status = OrderStatus.Open
        };

        _mockCustomerService.Setup(s => s.GetCustomerAsync(1)).ReturnsAsync(customer);
        _mockMapper.Setup(m => m.ToDomain(createOrderDto)).Returns(order);
        _mockRepository.Setup(r => r.CreateOrderAsync(order)).ReturnsAsync(order);
        _mockMapper.Setup(m => m.ToDto(order)).Returns(expectedOrderDto);

        // Act
        var result = await _orderService.CreateOrderAsync(createOrderDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedOrderDto.Id, result.Id);
        Assert.Equal(expectedOrderDto.CustomerId, result.CustomerId);
        Assert.Equal(expectedOrderDto.Status, result.Status);

        _mockCustomerService.Verify(s => s.GetCustomerAsync(1), Times.Once);
        _mockMapper.Verify(m => m.ToDomain(createOrderDto), Times.Once);
        _mockRepository.Verify(r => r.CreateOrderAsync(order), Times.Once);
        _mockMapper.Verify(m => m.ToDto(order), Times.Once);
    }

    [Fact]
    public async Task CreateOrderAsync_NonExistingCustomer_ThrowsArgumentException()
    {
        // Arrange
        var createOrderDto = new CreateOrderDto
        {
            CustomerId = 999,
            PickupDate = DateTime.UtcNow.AddDays(1)
        };

        _mockCustomerService.Setup(s => s.GetCustomerAsync(999)).ReturnsAsync((CustomerDto?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _orderService.CreateOrderAsync(createOrderDto));
        Assert.Contains("Customer with ID 999 does not exist", exception.Message);

        _mockCustomerService.Verify(s => s.GetCustomerAsync(999), Times.Once);
        _mockMapper.Verify(m => m.ToDomain(It.IsAny<CreateOrderDto>()), Times.Never);
        _mockRepository.Verify(r => r.CreateOrderAsync(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task CreateOrderAsync_PastPickupDate_ThrowsArgumentException()
    {
        // Arrange
        var createOrderDto = new CreateOrderDto
        {
            CustomerId = 1,
            PickupDate = DateTime.UtcNow.AddDays(-1)
        };

        var customer = new CustomerDto { Id = 1, FirstName = "John", LastName = "Doe" };
        _mockCustomerService.Setup(s => s.GetCustomerAsync(1)).ReturnsAsync(customer);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _orderService.CreateOrderAsync(createOrderDto));
        Assert.Contains("Pickup date must be today or in the future", exception.Message);

        _mockCustomerService.Verify(s => s.GetCustomerAsync(1), Times.Once);
        _mockMapper.Verify(m => m.ToDomain(It.IsAny<CreateOrderDto>()), Times.Never);
        _mockRepository.Verify(r => r.CreateOrderAsync(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task GetOrderAsync_ExistingOrder_ReturnsOrderDto()
    {
        // Arrange
        var orderId = 1;
        var order = new Order
        {
            Id = orderId,
            CustomerId = 1,
            PickupDate = DateTime.UtcNow.AddDays(1),
            Status = OrderStatus.Open,
            OrderPets = new List<OrderPet>
            {
                new OrderPet { PetId = 1, Pet = new Pet { Price = 500.00m } },
                new OrderPet { PetId = 2, Pet = new Pet { Price = 300.00m } }
            }
        };

        var expectedOrderDto = new OrderDto
        {
            Id = orderId,
            CustomerId = 1,
            CustomerName = "John Doe",
            PickupDate = DateTime.UtcNow.AddDays(1),
            Status = OrderStatus.Open,
            Cost = 800.00m,
            EstimatedCost = 800.00m
        };

        _mockRepository.Setup(r => r.GetOrderByIdAsync(orderId)).ReturnsAsync(order);
        _mockMapper.Setup(m => m.ToDto(order)).Returns(expectedOrderDto);

        // Act
        var result = await _orderService.GetOrderAsync(orderId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedOrderDto.Id, result.Id);
        Assert.Equal(expectedOrderDto.Cost, result.Cost);
        Assert.Equal(expectedOrderDto.EstimatedCost, result.EstimatedCost);

        _mockRepository.Verify(r => r.GetOrderByIdAsync(orderId), Times.Once);
        _mockMapper.Verify(m => m.ToDto(order), Times.Once);
    }

    [Fact]
    public async Task GetOrderAsync_DeliveredOrder_ReturnsOrderDtoWithActualCost()
    {
        // Arrange
        var orderId = 1;
        var order = new Order
        {
            Id = orderId,
            CustomerId = 1,
            PickupDate = DateTime.UtcNow.AddDays(1),
            Status = OrderStatus.Delivered,
            ActualCost = 750.00m,
            OrderPets = new List<OrderPet>
            {
                new OrderPet { PetId = 1, Pet = new Pet { Price = 500.00m } },
                new OrderPet { PetId = 2, Pet = new Pet { Price = 250.00m } }
            }
        };

        var expectedOrderDto = new OrderDto
        {
            Id = orderId,
            CustomerId = 1,
            CustomerName = "John Doe",
            PickupDate = DateTime.UtcNow.AddDays(1),
            Status = OrderStatus.Delivered,
            Cost = 750.00m,
            EstimatedCost = 750.00m
        };

        _mockRepository.Setup(r => r.GetOrderByIdAsync(orderId)).ReturnsAsync(order);
        _mockMapper.Setup(m => m.ToDto(order)).Returns(expectedOrderDto);

        // Act
        var result = await _orderService.GetOrderAsync(orderId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedOrderDto.Id, result.Id);
        Assert.Equal(expectedOrderDto.Cost, result.Cost);
        Assert.Equal(expectedOrderDto.EstimatedCost, result.EstimatedCost);

        _mockRepository.Verify(r => r.GetOrderByIdAsync(orderId), Times.Once);
        _mockMapper.Verify(m => m.ToDto(order), Times.Once);
    }

    [Fact]
    public async Task GetOrderAsync_NonExistingOrder_ReturnsNull()
    {
        // Arrange
        var orderId = 999;
        _mockRepository.Setup(r => r.GetOrderByIdAsync(orderId)).ReturnsAsync((Order?)null);

        // Act
        var result = await _orderService.GetOrderAsync(orderId);

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(r => r.GetOrderByIdAsync(orderId), Times.Once);
        _mockMapper.Verify(m => m.ToDto(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task GetAllOrdersAsync_ReturnsAllOrders()
    {
        // Arrange
        var orders = new List<Order>
        {
            new Order { Id = 1, CustomerId = 1, Status = OrderStatus.Open },
            new Order { Id = 2, CustomerId = 2, Status = OrderStatus.Processing }
        };

        var expectedOrderDtos = new List<OrderDto>
        {
            new OrderDto { Id = 1, CustomerId = 1, Status = OrderStatus.Open },
            new OrderDto { Id = 2, CustomerId = 2, Status = OrderStatus.Processing }
        };

        _mockRepository.Setup(r => r.GetAllOrdersAsync()).ReturnsAsync(orders);
        _mockMapper.Setup(m => m.ToDto(orders[0])).Returns(expectedOrderDtos[0]);
        _mockMapper.Setup(m => m.ToDto(orders[1])).Returns(expectedOrderDtos[1]);

        // Act
        var result = await _orderService.GetAllOrdersAsync();

        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.Equal(expectedOrderDtos[0].Id, resultList[0].Id);
        Assert.Equal(expectedOrderDtos[1].Id, resultList[1].Id);

        _mockRepository.Verify(r => r.GetAllOrdersAsync(), Times.Once);
        _mockMapper.Verify(m => m.ToDto(It.IsAny<Order>()), Times.Exactly(2));
    }

    [Fact]
    public async Task GetOrdersByCustomer_ExistingCustomer_ReturnsCustomerOrders()
    {
        // Arrange
        var customerId = 1;
        var customer = new CustomerDto { Id = customerId, FirstName = "John", LastName = "Doe" };
        var orders = new List<Order>
        {
            new Order { Id = 1, CustomerId = customerId, Status = OrderStatus.Open },
            new Order { Id = 2, CustomerId = customerId, Status = OrderStatus.Processing }
        };

        var expectedOrderDtos = new List<OrderDto>
        {
            new OrderDto { Id = 1, CustomerId = customerId, Status = OrderStatus.Open },
            new OrderDto { Id = 2, CustomerId = customerId, Status = OrderStatus.Processing }
        };

        _mockCustomerService.Setup(s => s.GetCustomerAsync(customerId)).ReturnsAsync(customer);
        _mockRepository.Setup(r => r.GetOrdersByCustomerAsync(customerId)).ReturnsAsync(orders);
        _mockMapper.Setup(m => m.ToDto(orders[0])).Returns(expectedOrderDtos[0]);
        _mockMapper.Setup(m => m.ToDto(orders[1])).Returns(expectedOrderDtos[1]);

        // Act
        var result = await _orderService.GetOrdersByCustomer(customerId);

        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.Equal(expectedOrderDtos[0].Id, resultList[0].Id);
        Assert.Equal(expectedOrderDtos[1].Id, resultList[1].Id);

        _mockCustomerService.Verify(s => s.GetCustomerAsync(customerId), Times.Once);
        _mockRepository.Verify(r => r.GetOrdersByCustomerAsync(customerId), Times.Once);
        _mockMapper.Verify(m => m.ToDto(It.IsAny<Order>()), Times.Exactly(2));
    }

    [Fact]
    public async Task GetOrdersByCustomer_NonExistingCustomer_ThrowsArgumentException()
    {
        // Arrange
        var customerId = 999;
        _mockCustomerService.Setup(s => s.GetCustomerAsync(customerId)).ReturnsAsync((CustomerDto?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _orderService.GetOrdersByCustomer(customerId));
        Assert.Contains("Customer with ID 999 does not exist", exception.Message);

        _mockCustomerService.Verify(s => s.GetCustomerAsync(customerId), Times.Once);
        _mockRepository.Verify(r => r.GetOrdersByCustomerAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task AddOrderPetAsync_ValidData_ReturnsUpdatedOrderDto()
    {
        // Arrange
        var createOrderPetDto = new CreateOrderPetDto
        {
            OrderId = 1,
            PetId = 1
        };

        var existingOrder = new Order
        {
            Id = 1,
            CustomerId = 1,
            Status = OrderStatus.Open,
            OrderPets = new List<OrderPet>()
        };

        var pet = new PetDto
        {
            Id = 1,
            Name = "Buddy",
            Price = 500.00m,
            Kind = PetKind.Dog
        };

        var orderPet = new OrderPet
        {
            OrderId = 1,
            PetId = 1
        };

        var updatedOrder = new Order
        {
            Id = 1,
            CustomerId = 1,
            Status = OrderStatus.Open,
            OrderPets = new List<OrderPet> { orderPet }
        };

        var expectedOrderDto = new OrderDto
        {
            Id = 1,
            CustomerId = 1,
            Status = OrderStatus.Open
        };

        _mockRepository.SetupSequence(r => r.GetOrderByIdAsync(1))
            .ReturnsAsync(existingOrder)
            .ReturnsAsync(updatedOrder);
        _mockPetService.Setup(s => s.GetPetAsync(1)).ReturnsAsync(pet);
        _mockMapper.Setup(m => m.ToDomain(createOrderPetDto)).Returns(orderPet);
        _mockRepository.Setup(r => r.AddOrderPetAsync(orderPet)).ReturnsAsync(orderPet);
        _mockMapper.Setup(m => m.ToDto(updatedOrder)).Returns(expectedOrderDto);

        // Act
        var result = await _orderService.AddOrderPetAsync(createOrderPetDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedOrderDto.Id, result.Id);

        _mockRepository.Verify(r => r.GetOrderByIdAsync(1), Times.Exactly(2));
        _mockPetService.Verify(s => s.GetPetAsync(1), Times.Once);
        _mockMapper.Verify(m => m.ToDomain(createOrderPetDto), Times.Once);
        _mockRepository.Verify(r => r.AddOrderPetAsync(orderPet), Times.Once);
        _mockMapper.Verify(m => m.ToDto(updatedOrder), Times.Once);
    }

    [Fact]
    public async Task AddOrderPetAsync_NonExistingOrder_ThrowsArgumentException()
    {
        // Arrange
        var createOrderPetDto = new CreateOrderPetDto
        {
            OrderId = 999,
            PetId = 1
        };

        _mockRepository.Setup(r => r.GetOrderByIdAsync(999)).ReturnsAsync((Order?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _orderService.AddOrderPetAsync(createOrderPetDto));
        Assert.Contains("Order with ID 999 does not exist", exception.Message);

        _mockRepository.Verify(r => r.GetOrderByIdAsync(999), Times.Once);
        _mockPetService.Verify(s => s.GetPetAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task AddOrderPetAsync_ClosedOrder_ThrowsArgumentException()
    {
        // Arrange
        var createOrderPetDto = new CreateOrderPetDto
        {
            OrderId = 1,
            PetId = 1
        };

        var existingOrder = new Order
        {
            Id = 1,
            CustomerId = 1,
            Status = OrderStatus.Delivered,
            OrderPets = new List<OrderPet>()
        };

        _mockRepository.Setup(r => r.GetOrderByIdAsync(1)).ReturnsAsync(existingOrder);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _orderService.AddOrderPetAsync(createOrderPetDto));
        Assert.Contains("Cannot add pet to order with ID 1 because it is not open", exception.Message);

        _mockRepository.Verify(r => r.GetOrderByIdAsync(1), Times.Once);
        _mockPetService.Verify(s => s.GetPetAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task AddOrderPetAsync_PetAlreadyInOrder_ThrowsArgumentException()
    {
        // Arrange
        var createOrderPetDto = new CreateOrderPetDto
        {
            OrderId = 1,
            PetId = 1
        };

        var existingOrder = new Order
        {
            Id = 1,
            CustomerId = 1,
            Status = OrderStatus.Open,
            OrderPets = new List<OrderPet>
            {
                new OrderPet { PetId = 1 }
            }
        };

        _mockRepository.Setup(r => r.GetOrderByIdAsync(1)).ReturnsAsync(existingOrder);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _orderService.AddOrderPetAsync(createOrderPetDto));
        Assert.Contains("Pet with ID 1 is already added to the order", exception.Message);

        _mockRepository.Verify(r => r.GetOrderByIdAsync(1), Times.Once);
        _mockPetService.Verify(s => s.GetPetAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task AddOrderPetAsync_NonExistingPet_ThrowsArgumentException()
    {
        // Arrange
        var createOrderPetDto = new CreateOrderPetDto
        {
            OrderId = 1,
            PetId = 999
        };

        var existingOrder = new Order
        {
            Id = 1,
            CustomerId = 1,
            Status = OrderStatus.Open,
            OrderPets = new List<OrderPet>()
        };

        _mockRepository.Setup(r => r.GetOrderByIdAsync(1)).ReturnsAsync(existingOrder);
        _mockPetService.Setup(s => s.GetPetAsync(999)).ReturnsAsync((PetDto?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _orderService.AddOrderPetAsync(createOrderPetDto));
        Assert.Contains("Pet with ID 999 does not exist", exception.Message);

        _mockRepository.Verify(r => r.GetOrderByIdAsync(1), Times.Once);
        _mockPetService.Verify(s => s.GetPetAsync(999), Times.Once);
    }

    [Fact]
    public async Task RemoveOrderPetAsync_ValidData_ReturnsUpdatedOrderDto()
    {
        // Arrange
        var removeOrderPetDto = new RemoveOrderPetDto
        {
            OrderId = 1,
            PetId = 1
        };

        var existingOrder = new Order
        {
            Id = 1,
            CustomerId = 1,
            Status = OrderStatus.Open,
            OrderPets = new List<OrderPet>
            {
                new OrderPet { PetId = 1 }
            }
        };

        var expectedOrderDto = new OrderDto
        {
            Id = 1,
            CustomerId = 1,
            Status = OrderStatus.Open
        };

        _mockRepository.Setup(r => r.GetOrderByIdAsync(1)).ReturnsAsync(existingOrder);
        _mockRepository.Setup(r => r.RemoveOrderPetAsync(It.IsAny<OrderPet>())).ReturnsAsync(new OrderPet());
        _mockMapper.Setup(m => m.ToDto(existingOrder)).Returns(expectedOrderDto);

        // Act
        var result = await _orderService.RemoveOrderPetAsync(removeOrderPetDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedOrderDto.Id, result.Id);

        _mockRepository.Verify(r => r.GetOrderByIdAsync(1), Times.Once);
        _mockRepository.Verify(r => r.RemoveOrderPetAsync(It.IsAny<OrderPet>()), Times.Once);
        _mockMapper.Verify(m => m.ToDto(existingOrder), Times.Once);
    }

    [Fact]
    public async Task RemoveOrderPetAsync_NonExistingOrder_ThrowsArgumentException()
    {
        // Arrange
        var removeOrderPetDto = new RemoveOrderPetDto
        {
            OrderId = 999,
            PetId = 1
        };

        _mockRepository.Setup(r => r.GetOrderByIdAsync(999)).ReturnsAsync((Order?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _orderService.RemoveOrderPetAsync(removeOrderPetDto));
        Assert.Contains("Order with ID 999 does not exist", exception.Message);

        _mockRepository.Verify(r => r.GetOrderByIdAsync(999), Times.Once);
        _mockRepository.Verify(r => r.RemoveOrderPetAsync(It.IsAny<OrderPet>()), Times.Never);
    }

    [Fact]
    public async Task RemoveOrderPetAsync_PetNotInOrder_ThrowsArgumentException()
    {
        // Arrange
        var removeOrderPetDto = new RemoveOrderPetDto
        {
            OrderId = 1,
            PetId = 999
        };

        var existingOrder = new Order
        {
            Id = 1,
            CustomerId = 1,
            Status = OrderStatus.Open,
            OrderPets = new List<OrderPet>
            {
                new OrderPet { PetId = 1 }
            }
        };

        _mockRepository.Setup(r => r.GetOrderByIdAsync(1)).ReturnsAsync(existingOrder);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _orderService.RemoveOrderPetAsync(removeOrderPetDto));
        Assert.Contains("Pet with ID 999 is not in the order", exception.Message);

        _mockRepository.Verify(r => r.GetOrderByIdAsync(1), Times.Once);
        _mockRepository.Verify(r => r.RemoveOrderPetAsync(It.IsAny<OrderPet>()), Times.Never);
    }

    [Fact]
    public async Task RemoveOrderPetAsync_ClosedOrder_ThrowsArgumentException()
    {
        // Arrange
        var removeOrderPetDto = new RemoveOrderPetDto
        {
            OrderId = 1,
            PetId = 1
        };

        var existingOrder = new Order
        {
            Id = 1,
            CustomerId = 1,
            Status = OrderStatus.Delivered,
            OrderPets = new List<OrderPet>
            {
                new OrderPet { PetId = 1 }
            }
        };

        _mockRepository.Setup(r => r.GetOrderByIdAsync(1)).ReturnsAsync(existingOrder);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _orderService.RemoveOrderPetAsync(removeOrderPetDto));
        Assert.Contains("Cannot remove pet from order with ID 1 because it is not open", exception.Message);

        _mockRepository.Verify(r => r.GetOrderByIdAsync(1), Times.Once);
        _mockRepository.Verify(r => r.RemoveOrderPetAsync(It.IsAny<OrderPet>()), Times.Never);
    }

    [Fact]
    public async Task UpdateOrderAsync_ValidData_ReturnsUpdatedOrderDto()
    {
        // Arrange
        var orderId = 1;
        var updateOrderDto = new UpdateOrderDto
        {
            PickupDate = DateTime.UtcNow.AddDays(2),
            Status = OrderStatus.Processing
        };

        var existingOrder = new Order
        {
            Id = orderId,
            CustomerId = 1,
            PickupDate = DateTime.UtcNow.AddDays(1),
            Status = OrderStatus.Open,
            OrderPets = new List<OrderPet>
            {
                new OrderPet { PetId = 1, Pet = new Pet { Price = 500.00m } }
            }
        };

        var expectedOrderDto = new OrderDto
        {
            Id = orderId,
            CustomerId = 1,
            PickupDate = DateTime.UtcNow.AddDays(2),
            Status = OrderStatus.Processing
        };

        _mockRepository.Setup(r => r.GetOrderByIdAsync(orderId)).ReturnsAsync(existingOrder);
        _mockRepository.Setup(r => r.UpdateOrderAsync(existingOrder)).ReturnsAsync(existingOrder);
        _mockMapper.Setup(m => m.ToDto(existingOrder)).Returns(expectedOrderDto);

        // Act
        var result = await _orderService.UpdateOrderAsync(orderId, updateOrderDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedOrderDto.Id, result.Id);
        Assert.Equal(expectedOrderDto.Status, result.Status);

        _mockRepository.Verify(r => r.GetOrderByIdAsync(orderId), Times.Once);
        _mockRepository.Verify(r => r.UpdateOrderAsync(existingOrder), Times.Once);
        _mockMapper.Verify(m => m.ToDto(existingOrder), Times.Once);
    }

    [Fact]
    public async Task UpdateOrderAsync_NonExistingOrder_ThrowsArgumentException()
    {
        // Arrange
        var orderId = 999;
        var updateOrderDto = new UpdateOrderDto
        {
            Status = OrderStatus.Processing
        };

        _mockRepository.Setup(r => r.GetOrderByIdAsync(orderId)).ReturnsAsync((Order?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _orderService.UpdateOrderAsync(orderId, updateOrderDto));
        Assert.Contains("Order with ID 999 does not exist", exception.Message);

        _mockRepository.Verify(r => r.GetOrderByIdAsync(orderId), Times.Once);
        _mockRepository.Verify(r => r.UpdateOrderAsync(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task UpdateOrderAsync_PastPickupDate_ThrowsArgumentException()
    {
        // Arrange
        var orderId = 1;
        var updateOrderDto = new UpdateOrderDto
        {
            PickupDate = DateTime.UtcNow.AddDays(-1)
        };

        var existingOrder = new Order
        {
            Id = orderId,
            CustomerId = 1,
            Status = OrderStatus.Open
        };

        _mockRepository.Setup(r => r.GetOrderByIdAsync(orderId)).ReturnsAsync(existingOrder);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _orderService.UpdateOrderAsync(orderId, updateOrderDto));
        Assert.Contains("Pickup date must be today or in the future", exception.Message);

        _mockRepository.Verify(r => r.GetOrderByIdAsync(orderId), Times.Once);
        _mockRepository.Verify(r => r.UpdateOrderAsync(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task UpdateOrderAsync_DeliveredOrder_ThrowsArgumentException()
    {
        // Arrange
        var orderId = 1;
        var updateOrderDto = new UpdateOrderDto
        {
            Status = OrderStatus.Processing
        };

        var existingOrder = new Order
        {
            Id = orderId,
            CustomerId = 1,
            Status = OrderStatus.Delivered
        };

        _mockRepository.Setup(r => r.GetOrderByIdAsync(orderId)).ReturnsAsync(existingOrder);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _orderService.UpdateOrderAsync(orderId, updateOrderDto));
        Assert.Contains("Cannot update order with ID 1 because it is Delivered", exception.Message);

        _mockRepository.Verify(r => r.GetOrderByIdAsync(orderId), Times.Once);
        _mockRepository.Verify(r => r.UpdateOrderAsync(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task UpdateOrderAsync_DeliveredStatusWithNoPets_ThrowsArgumentException()
    {
        // Arrange
        var orderId = 1;
        var updateOrderDto = new UpdateOrderDto
        {
            Status = OrderStatus.Delivered
        };

        var existingOrder = new Order
        {
            Id = orderId,
            CustomerId = 1,
            Status = OrderStatus.Open,
            OrderPets = new List<OrderPet>()
        };

        _mockRepository.Setup(r => r.GetOrderByIdAsync(orderId)).ReturnsAsync(existingOrder);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _orderService.UpdateOrderAsync(orderId, updateOrderDto));
        Assert.Contains("Cannot set order with ID 1 to Delivered because it has no pets", exception.Message);

        _mockRepository.Verify(r => r.GetOrderByIdAsync(orderId), Times.Once);
        _mockRepository.Verify(r => r.UpdateOrderAsync(It.IsAny<Order>()), Times.Never);
    }
}
