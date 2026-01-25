using Moq;
using PetShop.Application.DTOs;
using PetShop.Application.Features.Orders;
using PetShop.Application.Interfaces.Mappers;
using PetShop.Application.Interfaces.Services;
using PetShop.Domain.Entities;
using PetShop.Domain.Enums;
using PetShop.Domain.Exceptions;
using PetShop.Domain.Interfaces.Repositories;

namespace PetShop.Tests.UnitTests;

public class OrderServicesTests
{
    private readonly OrderService _orderService;
    private readonly Mock<IOrderRepository> _mockRepository;
    private readonly Mock<ICustomerService> _mockCustomerService;
    private readonly Mock<IPetService> _mockPetService;
    private readonly Mock<IOrderMapper> _orderMapper;

    public OrderServicesTests()
    {
        _mockRepository = new Mock<IOrderRepository>();
        _mockCustomerService = new Mock<ICustomerService>();
        _mockPetService = new Mock<IPetService>();
        _orderMapper = new Mock<IOrderMapper>();
        _orderService = new OrderService(_mockRepository.Object, _orderMapper.Object, _mockCustomerService.Object, _mockPetService.Object);
    }
    
    [Fact]
    public async Task AddOrderPetAsync_ReturnsOrderDto_WhenSuccessful()
    {
        // Arrange
        var createOrderDto = new CreateOrderDto()
        {
            CustomerId = Guid.NewGuid(),
            PickupDate =  DateTime.UtcNow.AddDays(2)
        };
        
        //create customer
        var customerDto = new CustomerDto()
        {
            Id = createOrderDto.CustomerId,
            FirstName = "John",
            LastName = "Doe",
            Email = "test@test.com",
            Phone = "1234567890"
        };
        
        //create order
        var order = new Order()
        {
            Id = Guid.NewGuid(),
            CustomerId = createOrderDto.CustomerId,
            Status = OrderStatus.Open,
            PickupDate = createOrderDto.PickupDate,
            OrderPets = new List<OrderPet>()
        };
        
        //expected order dto
        var expectedOrderDto = new OrderDto()
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            CustomerName = $"{customerDto.FirstName} {customerDto.LastName}",
            Status = order.Status,
            PickupDate = order.PickupDate,
        };
        
        _mockRepository.Setup(r => r.CreateOrderAsync(order)).ReturnsAsync(order);
        _orderMapper.Setup(m => m.ToDomain(createOrderDto)).Returns(order);
        _mockCustomerService.Setup(c => c.GetCustomerAsync(createOrderDto.CustomerId)).ReturnsAsync(customerDto);
        _orderMapper.Setup(m => m.ToDto(order)).Returns(expectedOrderDto);
        
        // Act
        var result = await _orderService.CreateOrderAsync(createOrderDto);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedOrderDto.Id, result.Id);
        Assert.Equal(expectedOrderDto.CustomerId, result.CustomerId);
        Assert.Equal(expectedOrderDto.CustomerName, result.CustomerName);
        
        _mockCustomerService.Verify(c => c.GetCustomerAsync(createOrderDto.CustomerId), Times.Once);
        _orderMapper.Verify(m => m.ToDto(order), Times.Once);
        _mockRepository.Verify(r => r.CreateOrderAsync(order), Times.Once);
        _orderMapper.Verify(m => m.ToDto(order), Times.Once);
     
    }

    [Fact]
    public async Task CreateOrderAsync_ThrowsAppException_WhenCustomerDoesNotExist()
    {
        // Arrange
        var createOrderDto = new CreateOrderDto()
        {
            CustomerId = Guid.NewGuid(),
            PickupDate = DateTime.UtcNow.AddDays(2)
        };

        _mockCustomerService.Setup(c => c.GetCustomerAsync(createOrderDto.CustomerId)).ReturnsAsync((CustomerDto?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _orderService.CreateOrderAsync(createOrderDto));
        Assert.Equal("Customer does not exist.", exception.Message);
        
        _mockCustomerService.Verify(c => c.GetCustomerAsync(createOrderDto.CustomerId), Times.Once);
        _mockRepository.Verify(r => r.CreateOrderAsync(It.IsAny<Order>()), Times.Never);
        _orderMapper.Verify(m => m.ToDto(It.IsAny<Order>()), Times.Never);
    }
    
    [Fact]
    public async Task CreateOrderAsync_ThrowsAppException_WhenPickupDateIsInThePast()
    {
        // Arrange
        var createOrderDto = new CreateOrderDto()
        {
            CustomerId = Guid.NewGuid(),
            PickupDate = DateTime.UtcNow.AddDays(-1) // Past date
        };
        
        _mockCustomerService.Setup(c => c.GetCustomerAsync(createOrderDto.CustomerId)).ReturnsAsync(new CustomerDto()
        {
            Id = createOrderDto.CustomerId,
            FirstName = "Jane",
            LastName = "Doe",
            Email = "test@test.com",
            Phone = "0987654321"
        });
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _orderService.CreateOrderAsync(createOrderDto));
        Assert.Equal("Pickup date must be today or sometime in the future.", exception.Message);
        
        _mockCustomerService.Verify(c => c.GetCustomerAsync(createOrderDto.CustomerId), Times.Once);
        _mockRepository.Verify(r => r.CreateOrderAsync(It.IsAny<Order>()), Times.Never);
        _orderMapper.Verify(m => m.ToDto(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task GetAllOrdersAsync_ReturnsListOfOrderDtos()
    {
        // Arrange
        var orders = new List<Order>()
        {
            new Order()
            {
                Id = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                Status = OrderStatus.Open,
                PickupDate = DateTime.UtcNow.AddDays(2)
            },
            new Order()
            {
                Id = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                Status = OrderStatus.Delivered,
                PickupDate = DateTime.UtcNow.AddDays(3)
            }
        };

        var expectedOrderDtos = orders.Select(o => new OrderDto()
        {
            Id = o.Id,
            CustomerId = o.CustomerId,
            Status = o.Status,
            PickupDate = o.PickupDate
        }).ToList();

        _mockRepository.Setup(r => r.GetAllOrdersAsync()).ReturnsAsync(orders);
        for (int i = 0; i < orders.Count; i++)
        {
            _orderMapper.Setup(m => m.ToDto(orders[i])).Returns(expectedOrderDtos[i]);
        }

        // Act
        var result = await _orderService.GetAllOrdersAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedOrderDtos.Count, result.Count);
        for (int i = 0; i < expectedOrderDtos.Count; i++)
        {
            Assert.Equal(expectedOrderDtos[i].Id, result[i].Id);
            Assert.Equal(expectedOrderDtos[i].CustomerId, result[i].CustomerId);
            Assert.Equal(expectedOrderDtos[i].Status, result[i].Status);
            Assert.Equal(expectedOrderDtos[i].PickupDate, result[i].PickupDate);

        }

        _mockRepository.Verify(r => r.GetAllOrdersAsync(), Times.Once);
        _orderMapper.Verify(m => m.ToDto(It.IsAny<Order>()), Times.Exactly(orders.Count));

    }
    
    [Fact]
    public async Task GetAllOrdersAsync_ReturnsEmptyList_WhenNoOrdersExist()
    {
        // Arrange
        var orders = new List<Order>();
        _mockRepository.Setup(r => r.GetAllOrdersAsync()).ReturnsAsync(orders);
        
        // Act
        var result = await _orderService.GetAllOrdersAsync();
        
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        
        _mockRepository.Verify(r => r.GetAllOrdersAsync(), Times.Once);
        _orderMapper.Verify(m => m.ToDto(It.IsAny<Order>()), Times.Never);
    }
    
    [Fact]
    public async Task GetAllOrdersAsync_ThrowsException_WhenRepositoryFails()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllOrdersAsync()).ThrowsAsync(new Exception("Database error"));
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _orderService.GetAllOrdersAsync());
        Assert.Equal("Database error", exception.Message);
        
        _mockRepository.Verify(r => r.GetAllOrdersAsync(), Times.Once);
        _orderMapper.Verify(m => m.ToDto(It.IsAny<Order>()), Times.Never);
    }


    [Fact]
    public async Task GetOrderAsync_ReturnsOrderDto_WhenOrderExists()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order()
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            Status = OrderStatus.Open,
            PickupDate = DateTime.UtcNow.AddDays(2)
        };

        var expectedOrderDto = new OrderDto()
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            Status = order.Status,
            PickupDate = order.PickupDate
        };

        _mockRepository.Setup(r => r.GetOrderByIdAsync(orderId)).ReturnsAsync(order);
        _orderMapper.Setup(m => m.ToDto(order)).Returns(expectedOrderDto);

        // Act
        var result = await _orderService.GetOrderAsync(orderId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedOrderDto.Id, result!.Id);
        Assert.Equal(expectedOrderDto.CustomerId, result.CustomerId);
        Assert.Equal(expectedOrderDto.Status, result.Status);
        Assert.Equal(expectedOrderDto.PickupDate, result.PickupDate);

        _mockRepository.Verify(r => r.GetOrderByIdAsync(orderId), Times.Once);
        _orderMapper.Verify(m => m.ToDto(order), Times.Once);
    }
    
    
    [Fact]
    public async Task GetOrderAsync_ThrowsException_WhenRepositoryFails()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetOrderByIdAsync(orderId)).ThrowsAsync(new Exception("Database error"));
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _orderService.GetOrderAsync(orderId));
        Assert.Equal("Database error", exception.Message);
        
        _mockRepository.Verify(r => r.GetOrderByIdAsync(orderId), Times.Once);
        _orderMapper.Verify(m => m.ToDto(It.IsAny<Order>()), Times.Never);
    }
    
    [Fact]
    public async Task GetOrderAsync_ThrowsException_WhenCustomerServiceFails()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order()
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            Status = OrderStatus.Open,
            PickupDate = DateTime.UtcNow.AddDays(2)
        };
        
        _mockRepository.Setup(r => r.GetOrderByIdAsync(orderId)).ReturnsAsync(order);
        _mockCustomerService.Setup(c => c.GetCustomerAsync(order.CustomerId)).ThrowsAsync(new Exception("Customer service error"));
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _orderService.GetOrderAsync(orderId));
        Assert.Equal("Customer service error", exception.Message);
        
        _mockRepository.Verify(r => r.GetOrderByIdAsync(orderId), Times.Once);
        _mockCustomerService.Verify(c => c.GetCustomerAsync(order.CustomerId), Times.Once);
        _orderMapper.Verify(m => m.ToDto(It.IsAny<Order>()), Times.Never);
    }
    
    [Fact]
    public async Task GetOrderAsync_ThrowsAppException_WhenOrderDoesNotExist()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetOrderByIdAsync(orderId)).ReturnsAsync((Order?)null);
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _orderService.GetOrderAsync(orderId));
        Assert.Equal($"Order not found.", exception.Message);
        
        _mockRepository.Verify(r => r.GetOrderByIdAsync(orderId), Times.Once);
        _orderMapper.Verify(m => m.ToDto(It.IsAny<Order>()), Times.Never);
    }
    
    [Fact]
    public async Task CreateOrderAsync_ThrowsAppException_WhenPickupDateIsToday()
    {
        // Arrange
        var createOrderDto = new CreateOrderDto()
        {
            CustomerId = Guid.NewGuid(),
            PickupDate = DateTime.UtcNow.AddDays(-2) // Today
        };  
        
        _mockCustomerService.Setup(c => c.GetCustomerAsync(createOrderDto.CustomerId)).ReturnsAsync(new CustomerDto()
        {
            Id = createOrderDto.CustomerId,
            FirstName = "Jane",
            LastName = "Doe",
            Email = "test@test.com",
            Phone = "0987654321",
            Address = "123 Main St"
            });
        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _orderService.CreateOrderAsync(createOrderDto));
        Assert.Equal("Pickup date must be today or sometime in the future.", exception.Message);    
        
        _mockCustomerService.Verify(c => c.GetCustomerAsync(createOrderDto.CustomerId), Times.Once);
        _mockRepository.Verify(r => r.CreateOrderAsync(It.IsAny<Order>()), Times.Never);
        _orderMapper.Verify(m => m.ToDto(It.IsAny<Order>()), Times.Never);
    }
    
    [Fact]
    public async Task CreateOrderAsync_ThrowsAppException_WhenPickupDateIsInThePast_DateTimeUtc()
    {
        // Arrange
        var createOrderDto = new CreateOrderDto()
        {
            CustomerId = Guid.NewGuid(),
            PickupDate = DateTime.UtcNow.AddDays(-1) // Past date
        };      
        
        _mockCustomerService.Setup(c => c.GetCustomerAsync(createOrderDto.CustomerId)).ReturnsAsync(new CustomerDto()
        {
            Id = createOrderDto.CustomerId,
            FirstName = "Jane",
            LastName = "Doe",
            Email = "text@test.com",
            Phone = "0987654321",
            Address = "123 Main St"
        });
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _orderService.CreateOrderAsync(createOrderDto));
        Assert.Equal("Pickup date must be today or sometime in the future.", exception.Message);    
        
        _mockCustomerService.Verify(c => c.GetCustomerAsync(createOrderDto.CustomerId), Times.Once);
        _mockRepository.Verify(r => r.CreateOrderAsync(It.IsAny<Order>()), Times.Never);
        _orderMapper.Verify(m => m.ToDto(It.IsAny<Order>()), Times.Never);
    }
    
    [Fact]
    public async Task CreateOrderAsync_ThrowsAppException_WhenCustomerServiceFails()
    {
        // Arrange
        var createOrderDto = new CreateOrderDto()
        {
            CustomerId = Guid.NewGuid(),
            PickupDate = DateTime.UtcNow.AddDays(2)
        };  
        
        _mockCustomerService.Setup(c => c.GetCustomerAsync(createOrderDto.CustomerId)).ThrowsAsync(new Exception("Customer service error"));
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _orderService.CreateOrderAsync(createOrderDto));
        Assert.Equal("Customer service error", exception.Message);
        
        _mockCustomerService.Verify(c => c.GetCustomerAsync(createOrderDto.CustomerId), Times.Once);
        _mockRepository.Verify(r => r.CreateOrderAsync(It.IsAny<Order>()), Times.Never);
        _orderMapper.Verify(m => m.ToDto(It.IsAny<Order>()), Times.Never);
    }
    
    [Fact]
    public async Task CreateOrderAsync_ThrowsException_WhenRepositoryFails()
    {
        // Arrange
        var createOrderDto = new CreateOrderDto()
        {
            CustomerId = Guid.NewGuid(),
            PickupDate = DateTime.UtcNow.AddDays(2)
        };  
        
        _mockCustomerService.Setup(c => c.GetCustomerAsync(createOrderDto.CustomerId)).ReturnsAsync(new CustomerDto()
        {
            Id = createOrderDto.CustomerId,
            FirstName = "Jane",
            LastName = "Doe",
            Email = "test@test.com",
            Phone = "0987654321",
            Address = "123 Main St"
        });
        
        _orderMapper.Setup(m => m.ToDomain(createOrderDto)).Returns(new Order());
        _mockRepository.Setup(r => r.CreateOrderAsync(It.IsAny<Order>())).ThrowsAsync(new Exception("Database error"));
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _orderService.CreateOrderAsync(createOrderDto));
        Assert.Equal("Database error", exception.Message);
        
        _mockCustomerService.Verify(c => c.GetCustomerAsync(createOrderDto.CustomerId), Times.Once);
        _mockRepository.Verify(r => r.CreateOrderAsync(It.IsAny<Order>()), Times.Once);
        _orderMapper.Verify(m => m.ToDto(It.IsAny<Order>()), Times.Never);
    }
    
    [Fact]
    public async Task UpdateOrderAsync_ThrowsAppException_WhenOrderDoesNotExist()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var updateOrderDto = new UpdateOrderDto()
        {
            PickupDate = DateTime.UtcNow.AddDays(5),
            Status = OrderStatus.Delivered
        };

        _mockRepository.Setup(r => r.GetOrderByIdAsync(orderId)).ReturnsAsync((Order?)null);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<AppException>(() => _orderService.UpdateOrderAsync(orderId, updateOrderDto));
        Assert.Equal($"Order with the ID {orderId} does not exist.", exception.Message);

        _mockRepository.Verify(r => r.GetOrderByIdAsync(orderId), Times.Once);
        _mockRepository.Verify(r => r.UpdateOrderAsync(It.IsAny<Order>()), Times.Never);
        _orderMapper.Verify(m => m.ToDto(It.IsAny<Order>()), Times.Never);

    }
    
    [Fact]
    public async Task UpdateOrderAsync_ThrowsAppException_WhenOrderIsDelivered()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var updateOrderDto = new UpdateOrderDto()
        {
            PickupDate = DateTime.UtcNow.AddDays(5),    
            Status = OrderStatus.Open
        };
        
        var existingOrder = new Order()
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            PickupDate = DateTime.UtcNow.AddDays(2),
            Status = OrderStatus.Delivered
        };
        
        _mockRepository.Setup(r => r.GetOrderByIdAsync(orderId)).ReturnsAsync(existingOrder);
        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _orderService.UpdateOrderAsync(orderId, updateOrderDto));
        Assert.Equal($"Cannot update order with ID {orderId} because it is Delivered.", exception.Message);
        
        _mockRepository.Verify(r => r.GetOrderByIdAsync(orderId), Times.Once);
        _mockRepository.Verify(r => r.UpdateOrderAsync(It.IsAny<Order>()), Times.Never);
        _orderMapper.Verify(m => m.ToDto(It.IsAny<Order>()), Times.Never);
    }
    
    [Fact]
    public async Task UpdateOrderAsync_ThrowsAppException_WhenSettingDeliveredWithNoPets()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var updateOrderDto = new UpdateOrderDto()
        {
            PickupDate = DateTime.UtcNow.AddDays(5),    
            Status = OrderStatus.Delivered
        };  
        
        var existingOrder = new Order()
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            PickupDate = DateTime.UtcNow.AddDays(2),
            Status = OrderStatus.Processing,
            OrderPets = new List<OrderPet>()
        };
        
        _mockRepository.Setup(r => r.GetOrderByIdAsync(orderId)).ReturnsAsync(existingOrder);
        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _orderService.UpdateOrderAsync(orderId, updateOrderDto));
        Assert.Equal($"Cannot set order with the ID {orderId} to Delivered because it has no pets.", exception.Message);
        
        _mockRepository.Verify(r => r.GetOrderByIdAsync(orderId), Times.Once);
        _mockRepository.Verify(r => r.UpdateOrderAsync(It.IsAny<Order>()), Times.Never);
        _orderMapper.Verify(m => m.ToDto(It.IsAny<Order>()), Times.Never);
    }
    
    [Fact]
    public async Task UpdateOrderAsync_ThrowsAppException_WhenSettingProcessingWithPickupDate()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var updateOrderDto = new UpdateOrderDto()
        {
            PickupDate = DateTime.UtcNow.AddDays(5),    
            Status = OrderStatus.Processing
        };
        
        var existingOrder = new Order()
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            PickupDate = DateTime.UtcNow.AddDays(2),
            Status = OrderStatus.Open
        };
        
        _mockRepository.Setup(r => r.GetOrderByIdAsync(orderId)).ReturnsAsync(existingOrder);
        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _orderService.UpdateOrderAsync(orderId, updateOrderDto));
        Assert.Equal("Cannot set pickup date when status is Processing.", exception.Message);
        
        _mockRepository.Verify(r => r.GetOrderByIdAsync(orderId), Times.Once);
        _mockRepository.Verify(r => r.UpdateOrderAsync(It.IsAny<Order>()), Times.Never);
        _orderMapper.Verify(m => m.ToDto(It.IsAny<Order>()), Times.Never);
    }
    
    [Fact]
    public async Task UpdateOrderAsync_ThrowsAppException_WhenPickupDateIsInThePast()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var updateOrderDto = new UpdateOrderDto()
        {
            PickupDate = DateTime.UtcNow.AddDays(-1),    
            Status = OrderStatus.Open
        };
        
        var existingOrder = new Order()
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            PickupDate = DateTime.UtcNow.AddDays(2),
            Status = OrderStatus.Open
        };
        
        _mockRepository.Setup(r => r.GetOrderByIdAsync(orderId)).ReturnsAsync(existingOrder);
        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _orderService.UpdateOrderAsync(orderId, updateOrderDto));
        Assert.Equal("Pickup date must be today or in the future.", exception.Message);
        
        _mockRepository.Verify(r => r.GetOrderByIdAsync(orderId), Times.Once);
        _mockRepository.Verify(r => r.UpdateOrderAsync(It.IsAny<Order>()), Times.Never);
        _orderMapper.Verify(m => m.ToDto(It.IsAny<Order>()), Times.Never);
    }
    
    [Fact]
    public async Task AddOrderPetAsync_ThrowsAppException_WhenPetDoesNotExist()
    {
        // Arrange
        var addOrderPetDto = new CreateOrderPetDto()
        {
            OrderId = Guid.NewGuid(),
            PetId = Guid.NewGuid()
        };
        
        var existingOrder = new Order()
        {
            Id = addOrderPetDto.OrderId,
            CustomerId = Guid.NewGuid(),
            Status = OrderStatus.Open,
            PickupDate = DateTime.UtcNow.AddDays(2),
            OrderPets = new List<OrderPet>()
        };
        
        _mockRepository.Setup(r => r.GetOrderByIdAsync(addOrderPetDto.OrderId)).ReturnsAsync(existingOrder);
        _mockPetService.Setup(p => p.GetPetAsync(addOrderPetDto.PetId)).ReturnsAsync((PetDto?)null);
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _orderService.AddOrderPetAsync(addOrderPetDto));
        Assert.Equal($"Pet with ID {addOrderPetDto.PetId} does not exist.", exception.Message);
        
        _mockRepository.Verify(r => r.GetOrderByIdAsync(addOrderPetDto.OrderId), Times.Once);
        _mockPetService.Verify(p => p.GetPetAsync(addOrderPetDto.PetId), Times.Once);
    }
    
    [Fact]
    public async Task AddOrderPetAsync_ThrowsAppException_WhenOrderDoesNotExist()
    {
        // Arrange
        var addOrderPetDto = new CreateOrderPetDto()
        {
            OrderId = Guid.NewGuid(),
            PetId = Guid.NewGuid()
        };  
        
        _mockRepository.Setup(r => r.GetOrderByIdAsync(addOrderPetDto.OrderId)).ReturnsAsync((Order?)null);
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _orderService.AddOrderPetAsync(addOrderPetDto));
        Assert.Equal($"The Order with that ID {addOrderPetDto.OrderId} does not exist.", exception.Message);
        
        _mockRepository.Verify(r => r.GetOrderByIdAsync(addOrderPetDto.OrderId), Times.Once);
        _mockPetService.Verify(p => p.GetPetAsync(It.IsAny<Guid>()), Times.Never);
    }
    
    [Fact]
    public async Task AddOrderPetAsync_ThrowsAppException_WhenOrderIsNotOpen()
    {
        // Arrange
        var addOrderPetDto = new CreateOrderPetDto()
        {
            OrderId = Guid.NewGuid(),
            PetId = Guid.NewGuid()
        };
        
        var existingOrder = new Order()
        {
            Id = addOrderPetDto.OrderId,
            CustomerId = Guid.NewGuid(),
            Status = OrderStatus.Delivered,
            PickupDate = DateTime.UtcNow.AddDays(2),
            OrderPets = new List<OrderPet>()
        };
        
        _mockRepository.Setup(r => r.GetOrderByIdAsync(addOrderPetDto.OrderId)).ReturnsAsync(existingOrder);
        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _orderService.AddOrderPetAsync(addOrderPetDto));
        Assert.Equal($"Cannot add pet to order with ID {addOrderPetDto.OrderId} because it is not open.", exception.Message);
        
        _mockRepository.Verify(r => r.GetOrderByIdAsync(addOrderPetDto.OrderId), Times.Once);
        _mockPetService.Verify(p => p.GetPetAsync(It.IsAny<Guid>()), Times.Never);
    }
    
    [Fact]
    public async Task AddOrderPetAsync_ThrowsAppException_WhenPetAlreadyInOrder()
    {
        // Arrange
        var addOrderPetDto = new CreateOrderPetDto()
        {
            OrderId = Guid.NewGuid(),
            PetId = Guid.NewGuid()
        };
        
        var existingOrder = new Order()
        {
            Id = addOrderPetDto.OrderId,
            CustomerId = Guid.NewGuid(),
            Status = OrderStatus.Open,
            PickupDate = DateTime.UtcNow.AddDays(2),
            OrderPets = new List<OrderPet>()
            {
                new OrderPet()
                {
                    OrderId = addOrderPetDto.OrderId,
                    PetId = addOrderPetDto.PetId
                }
            }
        };
        
        _mockRepository.Setup(r => r.GetOrderByIdAsync(addOrderPetDto.OrderId)).ReturnsAsync(existingOrder);    
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _orderService.AddOrderPetAsync(addOrderPetDto));
        Assert.Equal($"Pet with ID {addOrderPetDto.PetId} is already added to the order.", exception.Message);
        
        _mockRepository.Verify(r => r.GetOrderByIdAsync(addOrderPetDto.OrderId), Times.Once);
        _mockPetService.Verify(p => p.GetPetAsync(It.IsAny<Guid>()), Times.Never);
    }
    
    [Fact]
    public async Task AddOrderPetAsync_ThrowsException_WhenRepositoryFails()
    {
        // Arrange
        var addOrderPetDto = new CreateOrderPetDto()
        {
            OrderId = Guid.NewGuid(),
            PetId = Guid.NewGuid()
        };  
        
        var existingOrder = new Order()
        {
            Id = addOrderPetDto.OrderId,
            CustomerId = Guid.NewGuid(),
            Status = OrderStatus.Open,
            PickupDate = DateTime.UtcNow.AddDays(2),
            OrderPets = new List<OrderPet>()
        };
        
        _mockRepository.Setup(r => r.GetOrderByIdAsync(addOrderPetDto.OrderId)).ReturnsAsync(existingOrder);
        _mockPetService.Setup(p => p.GetPetAsync(addOrderPetDto.PetId)).ReturnsAsync(new PetDto()
        {
            Id = addOrderPetDto.PetId,
            Name = "Fido",
            Breed = "Labrador",
            Kind = PetKind.Cat,
            AgeInMonths = 11,
            IsVaccinated = true,
            Color = "Brown",
            Description = "A friendly dog",
            Price = 500.00m
        });
        
        _mockRepository.Setup(r => r.AddOrderPetAsync(It.IsAny<OrderPet>())).ThrowsAsync(new Exception("Database error"));
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _orderService.AddOrderPetAsync(addOrderPetDto));
        Assert.Equal("Database error", exception.Message);
        
        _mockRepository.Verify(r => r.GetOrderByIdAsync(addOrderPetDto.OrderId), Times.Once);
        _mockPetService.Verify(p => p.GetPetAsync(addOrderPetDto.PetId), Times.Once);
        _mockRepository.Verify(r => r.AddOrderPetAsync(It.IsAny<OrderPet>()), Times.Once);
    }


}