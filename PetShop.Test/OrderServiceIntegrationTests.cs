using PetShop.Application.DTOs;
using PetShop.Domain.Enums;
using Xunit;

namespace PetShop.Test;

public class OrderServiceIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task CreateOrderAsync_ValidData_CreatesOrderInDatabase()
    {
        // Arrange
        var customer = await CreateTestCustomerAsync();
        var createOrderDto = new CreateOrderDto
        {
            CustomerId = customer.Id,
            PickupDate = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var result = await _orderService.CreateOrderAsync(createOrderDto);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(0, result.Id);
        Assert.Equal(customer.Id, result.CustomerId);
        Assert.Equal(createOrderDto.PickupDate.Date, result.PickupDate.Date);
        Assert.Equal(OrderStatus.Open, result.Status);

        // Verify order was saved to database
        var savedOrder = await _orderService.GetOrderAsync(result.Id);
        Assert.NotNull(savedOrder);
        Assert.Equal(result.Id, savedOrder.Id);
        Assert.Equal(result.CustomerId, savedOrder.CustomerId);
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

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _orderService.CreateOrderAsync(createOrderDto));
        Assert.Contains("Customer with ID 999 does not exist", exception.Message);
    }

    [Fact]
    public async Task CreateOrderAsync_PastPickupDate_ThrowsArgumentException()
    {
        // Arrange
        var customer = await CreateTestCustomerAsync();
        var createOrderDto = new CreateOrderDto
        {
            CustomerId = customer.Id,
            PickupDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _orderService.CreateOrderAsync(createOrderDto));
        Assert.Contains("Pickup date must be today or in the future", exception.Message);
    }

    [Fact]
    public async Task GetOrderAsync_ExistingOrder_ReturnsOrder()
    {
        // Arrange
        var customer = await CreateTestCustomerAsync();
        var order = await CreateTestOrderAsync(customer.Id);

        // Act
        var result = await _orderService.GetOrderAsync(order.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(order.Id, result.Id);
        Assert.Equal(order.CustomerId, result.CustomerId);
        Assert.Equal(order.Status, result.Status);
    }

    [Fact]
    public async Task GetOrderAsync_NonExistingOrder_ReturnsNull()
    {
        // Act
        var result = await _orderService.GetOrderAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllOrdersAsync_ReturnsAllOrders()
    {
        // Arrange
        var customer1 = await CreateTestCustomerAsync();
        var customer2 = await CreateTestCustomerAsync();
        var order1 = await CreateTestOrderAsync(customer1.Id);
        var order2 = await CreateTestOrderAsync(customer2.Id);

        // Act
        var result = await _orderService.GetAllOrdersAsync();

        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();
        Assert.True(resultList.Count >= 2);
        Assert.Contains(resultList, o => o.Id == order1.Id);
        Assert.Contains(resultList, o => o.Id == order2.Id);
    }

    [Fact]
    public async Task GetOrdersByCustomer_ExistingCustomer_ReturnsCustomerOrders()
    {
        // Arrange
        var customer = await CreateTestCustomerAsync();
        var order1 = await CreateTestOrderAsync(customer.Id);
        var order2 = await CreateTestOrderAsync(customer.Id);

        // Act
        var result = await _orderService.GetOrdersByCustomer(customer.Id);

        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();
        Assert.True(resultList.Count >= 2);
        Assert.All(resultList, o => Assert.Equal(customer.Id, o.CustomerId));
        Assert.Contains(resultList, o => o.Id == order1.Id);
        Assert.Contains(resultList, o => o.Id == order2.Id);
    }

    [Fact]
    public async Task GetOrdersByCustomer_NonExistingCustomer_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _orderService.GetOrdersByCustomer(999));
        Assert.Contains("Customer with ID 999 does not exist", exception.Message);
    }

    [Fact]
    public async Task AddOrderPetAsync_ValidData_AddsPetToOrder()
    {
        // Arrange
        var customer = await CreateTestCustomerAsync();
        var order = await CreateTestOrderAsync(customer.Id);
        var pet = await CreateTestPetAsync();

        var createOrderPetDto = new CreateOrderPetDto
        {
            OrderId = order.Id,
            PetId = pet.Id
        };

        // Act
        var result = await _orderService.AddOrderPetAsync(createOrderPetDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(order.Id, result.Id);
        Assert.Equal(order.CustomerId, result.CustomerId);

        // Verify pet was added to order
        var updatedOrder = await _orderService.GetOrderAsync(order.Id);
        Assert.NotNull(updatedOrder);
        Assert.Contains(updatedOrder.Pets, p => p.Id == pet.Id);
    }

    [Fact]
    public async Task AddOrderPetAsync_NonExistingOrder_ThrowsArgumentException()
    {
        // Arrange
        var pet = await CreateTestPetAsync();
        var createOrderPetDto = new CreateOrderPetDto
        {
            OrderId = 999,
            PetId = pet.Id
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _orderService.AddOrderPetAsync(createOrderPetDto));
        Assert.Contains("Order with ID 999 does not exist", exception.Message);
    }

    [Fact]
    public async Task AddOrderPetAsync_NonExistingPet_ThrowsArgumentException()
    {
        // Arrange
        var customer = await CreateTestCustomerAsync();
        var order = await CreateTestOrderAsync(customer.Id);

        var createOrderPetDto = new CreateOrderPetDto
        {
            OrderId = order.Id,
            PetId = 999
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _orderService.AddOrderPetAsync(createOrderPetDto));
        Assert.Contains("Pet with ID 999 does not exist", exception.Message);
    }

    [Fact]
    public async Task AddOrderPetAsync_PetAlreadyInOrder_ThrowsArgumentException()
    {
        // Arrange
        var customer = await CreateTestCustomerAsync();
        var order = await CreateTestOrderAsync(customer.Id);
        var pet = await CreateTestPetAsync();

        var createOrderPetDto = new CreateOrderPetDto
        {
            OrderId = order.Id,
            PetId = pet.Id
        };

        // Add pet to order first
        await _orderService.AddOrderPetAsync(createOrderPetDto);

        // Act & Assert - try to add the same pet again
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _orderService.AddOrderPetAsync(createOrderPetDto));
        Assert.Contains("Pet with ID", exception.Message);
        Assert.Contains("is already added to the order", exception.Message);
    }

    [Fact]
    public async Task RemoveOrderPetAsync_ValidData_RemovesPetFromOrder()
    {
        // Arrange
        var customer = await CreateTestCustomerAsync();
        var order = await CreateTestOrderAsync(customer.Id);
        var pet = await CreateTestPetAsync();

        var createOrderPetDto = new CreateOrderPetDto
        {
            OrderId = order.Id,
            PetId = pet.Id
        };

        // Add pet to order first
        await _orderService.AddOrderPetAsync(createOrderPetDto);

        var removeOrderPetDto = new RemoveOrderPetDto
        {
            OrderId = order.Id,
            PetId = pet.Id
        };

        // Act
        var result = await _orderService.RemoveOrderPetAsync(removeOrderPetDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(order.Id, result.Id);

        // Verify pet was removed from order
        var updatedOrder = await _orderService.GetOrderAsync(order.Id);
        Assert.NotNull(updatedOrder);
        Assert.DoesNotContain(updatedOrder.Pets, p => p.Id == pet.Id);
    }

    [Fact]
    public async Task RemoveOrderPetAsync_NonExistingOrder_ThrowsArgumentException()
    {
        // Arrange
        var pet = await CreateTestPetAsync();
        var removeOrderPetDto = new RemoveOrderPetDto
        {
            OrderId = 999,
            PetId = pet.Id
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _orderService.RemoveOrderPetAsync(removeOrderPetDto));
        Assert.Contains("Order with ID 999 does not exist", exception.Message);
    }

    [Fact]
    public async Task RemoveOrderPetAsync_PetNotInOrder_ThrowsArgumentException()
    {
        // Arrange
        var customer = await CreateTestCustomerAsync();
        var order = await CreateTestOrderAsync(customer.Id);
        var pet = await CreateTestPetAsync();

        var removeOrderPetDto = new RemoveOrderPetDto
        {
            OrderId = order.Id,
            PetId = pet.Id
        };

        // Act & Assert - try to remove pet that's not in the order
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _orderService.RemoveOrderPetAsync(removeOrderPetDto));
        Assert.Contains("Pet with ID", exception.Message);
        Assert.Contains("is not in the order", exception.Message);
    }

    [Fact]
    public async Task UpdateOrderAsync_ValidData_UpdatesOrderInDatabase()
    {
        // Arrange
        var customer = await CreateTestCustomerAsync();
        var order = await CreateTestOrderAsync(customer.Id);

        var updateOrderDto = new UpdateOrderDto
        {
            PickupDate = DateTime.UtcNow.AddDays(2),
            Status = OrderStatus.Processing
        };

        // Act
        var result = await _orderService.UpdateOrderAsync(order.Id, updateOrderDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(order.Id, result.Id);
        Assert.Equal(updateOrderDto.PickupDate.Value.Date, result.PickupDate.Date);
        Assert.Equal(updateOrderDto.Status, result.Status);

        // Verify order was updated in database
        var updatedOrder = await _orderService.GetOrderAsync(order.Id);
        Assert.NotNull(updatedOrder);
        Assert.Equal(updateOrderDto.PickupDate.Value.Date, updatedOrder.PickupDate.Date);
        Assert.Equal(updateOrderDto.Status, updatedOrder.Status);
    }

    [Fact]
    public async Task UpdateOrderAsync_NonExistingOrder_ThrowsArgumentException()
    {
        // Arrange
        var updateOrderDto = new UpdateOrderDto
        {
            Status = OrderStatus.Processing
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _orderService.UpdateOrderAsync(999, updateOrderDto));
        Assert.Contains("Order with ID 999 does not exist", exception.Message);
    }

    [Fact]
    public async Task UpdateOrderAsync_PastPickupDate_ThrowsArgumentException()
    {
        // Arrange
        var customer = await CreateTestCustomerAsync();
        var order = await CreateTestOrderAsync(customer.Id);

        var updateOrderDto = new UpdateOrderDto
        {
            PickupDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _orderService.UpdateOrderAsync(order.Id, updateOrderDto));
        Assert.Contains("Pickup date must be today or in the future", exception.Message);
    }

    [Fact]
    public async Task UpdateOrderAsync_DeliveredStatusWithNoPets_ThrowsArgumentException()
    {
        // Arrange
        var customer = await CreateTestCustomerAsync();
        var order = await CreateTestOrderAsync(customer.Id);

        var updateOrderDto = new UpdateOrderDto
        {
            Status = OrderStatus.Delivered
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _orderService.UpdateOrderAsync(order.Id, updateOrderDto));
        Assert.Contains("Cannot set order with ID", exception.Message);
        Assert.Contains("to Delivered because it has no pets", exception.Message);
    }

    [Fact]
    public async Task OrderCRUD_CompleteWorkflow_Succeeds()
    {
        // Create customer and pet
        var customer = await CreateTestCustomerAsync();
        var pet = await CreateTestPetAsync();

        // Create order
        var createOrderDto = new CreateOrderDto
        {
            CustomerId = customer.Id,
            PickupDate = DateTime.UtcNow.AddDays(1)
        };

        var createdOrder = await _orderService.CreateOrderAsync(createOrderDto);
        Assert.NotNull(createdOrder);
        Assert.NotEqual(0, createdOrder.Id);
        Assert.Equal(OrderStatus.Open, createdOrder.Status);

        // Read order
        var retrievedOrder = await _orderService.GetOrderAsync(createdOrder.Id);
        Assert.NotNull(retrievedOrder);
        Assert.Equal(createdOrder.Id, retrievedOrder.Id);
        Assert.Equal(customer.Id, retrievedOrder.CustomerId);

        // Add pet to order
        var createOrderPetDto = new CreateOrderPetDto
        {
            OrderId = createdOrder.Id,
            PetId = pet.Id
        };

        var orderWithPet = await _orderService.AddOrderPetAsync(createOrderPetDto);
        Assert.NotNull(orderWithPet);

        // Verify order has pet and correct status
        var verifyOrder = await _orderService.GetOrderAsync(createdOrder.Id);
        Assert.NotNull(verifyOrder);
        Assert.Equal(OrderStatus.Open, verifyOrder.Status);
        Assert.Contains(verifyOrder.Pets, p => p.Id == pet.Id);

        // Remove pet from order
        var removeOrderPetDto = new RemoveOrderPetDto
        {
            OrderId = createdOrder.Id,
            PetId = pet.Id
        };

        var orderWithoutPet = await _orderService.RemoveOrderPetAsync(removeOrderPetDto);
        Assert.NotNull(orderWithoutPet);

        // Verify pet was removed
        var finalOrder = await _orderService.GetOrderAsync(createdOrder.Id);
        Assert.NotNull(finalOrder);
        Assert.DoesNotContain(finalOrder.Pets, p => p.Id == pet.Id);
    }

    [Fact]
    public async Task GetOrderAsync_OrderWithPets_CalculatesCorrectCost()
    {
        // Arrange
        var customer = await CreateTestCustomerAsync();
        var pet1 = await CreateTestPetAsync();
        var pet2 = await CreateTestPetAsync();

        var order = await CreateTestOrderAsync(customer.Id);

        // Add pets to order
        await _orderService.AddOrderPetAsync(new CreateOrderPetDto { OrderId = order.Id, PetId = pet1.Id });
        await _orderService.AddOrderPetAsync(new CreateOrderPetDto { OrderId = order.Id, PetId = pet2.Id });

        // Act
        var result = await _orderService.GetOrderAsync(order.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(pet1.Price + pet2.Price, result.Cost);
        Assert.Equal(pet1.Price + pet2.Price, result.EstimatedCost);
    }
} 