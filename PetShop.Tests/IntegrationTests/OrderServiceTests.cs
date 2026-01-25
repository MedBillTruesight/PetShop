using PetShop.Application.DTOs;
using PetShop.Domain.Enums;
using PetShop.Domain.Exceptions;

namespace PetShop.Tests.IntegrationTests;

public class OrderServiceTests : IntegrationTestBase
{
    [Fact]
    public async Task CreateOrderAsync_ShouldCreateOrderSuccessfully()
    {
        // Arrange
        var testCustomer = await CreateCustomerTestAsync();
        var newOrder = new CreateOrderDto()
        {
            CustomerId = testCustomer.Id,
            PickupDate = DateTime.UtcNow
        };

        // Act
        var createdOrder = await _orderService.CreateOrderAsync(newOrder);

        // Assert
        Assert.NotNull(createdOrder);
        Assert.Equal(newOrder.CustomerId, createdOrder.CustomerId);
        Assert.Equal(newOrder.PickupDate, createdOrder.PickupDate);

        // Verify that the order is actually in the database
        var fetchedOrder = await _orderService.GetOrderAsync(createdOrder.Id);
        Assert.NotNull(fetchedOrder);
        Assert.Equal(createdOrder.Id, fetchedOrder.Id);
        Assert.Equal(createdOrder.CustomerId, fetchedOrder.CustomerId);
        Assert.Equal(createdOrder.PickupDate, fetchedOrder.PickupDate);

    }

    [Fact]
    public async Task GetOrderAsync_ShouldReturnOrder_WhenOrderExists()
    {
        var newCustomer = new CreateCustomerDto()
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "test@test.com",
            Phone = "123-456-7890",
            Address = "123 Main St, Anytown, USA"
        };

        var customer1 = await _customerService.CreateCustomerAsync(newCustomer);
        // Arrange
        var testOrder = await CreateOrderTestAsync(customer1.Id);

        // Act
        var fetchedOrder = await _orderService.GetOrderAsync(testOrder.Id);

        // Assert
        Assert.NotNull(fetchedOrder);
        Assert.Equal(testOrder.Id, fetchedOrder.Id);
        Assert.Equal(testOrder.CustomerId, fetchedOrder.CustomerId);
        Assert.Equal(testOrder.PickupDate, fetchedOrder.PickupDate);

    }

    [Fact]
    public async Task GetAllOrdersAsync_ShouldReturnAllOrders()
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

        var order1 = await CreateOrderTestAsync(customer1.Id);
        var order2 = await CreateOrderTestAsync(customer1.Id);

        // Act
        var allOrders = await _orderService.GetAllOrdersAsync();

        // Assert
        Assert.NotNull(allOrders);
        Assert.True(allOrders.Count >= 2);
    }
    
    [Fact]
     public async Task AddPetToOrderAsync_ShouldAddPetToExistingOrder()
     {
         // Arrange
         var testCustomer = await CreateCustomerTestAsync();
         var testPet = await CreatePetTestAsync();
         var testOrder = await CreateOrderTestAsync(testCustomer.Id);

         var createOrderPetDto = new CreateOrderPetDto
         {
             OrderId = testOrder.Id,
             PetId = testPet.Id
         };

         // Act
         var updatedOrder = await _orderService.AddOrderPetAsync(createOrderPetDto);

         // Assert
         Assert.NotNull(updatedOrder);
         Assert.Equal(testOrder.Id, updatedOrder.Id);
         Assert.Contains(updatedOrder.Pets, pet => pet.Id == testPet.Id);
     }

    [Fact]
    public async Task AddPetToOrderAsync_ShouldThrowException_WhenOrderDoesNotExist()
    {
        // Arrange
        var testPet = await CreatePetTestAsync();
        var createOrderPetDto = new CreateOrderPetDto
        {
            OrderId = Guid.NewGuid(), // Non-existent order ID
            PetId = testPet.Id
        };

        // Act & Assert
        await Assert.ThrowsAsync<AppException>(async () =>
            await _orderService.AddOrderPetAsync(createOrderPetDto));
    }
    
    [Fact]
    public async Task RemovePetFromOrderAsync_ShouldRemovePetFromOrder()
    {
        // Arrange
        var testCustomer = await CreateCustomerTestAsync();
        var testPet = await CreatePetTestAsync();
        var testOrder = await CreateOrderTestAsync(testCustomer.Id);

        var createOrderPetDto = new CreateOrderPetDto
        {
            OrderId = testOrder.Id,
            PetId = testPet.Id
        };

        // Add pet to order first
        var updatedOrder = await _orderService.AddOrderPetAsync(createOrderPetDto);
        Assert.Contains(updatedOrder.Pets, pet => pet.Id == testPet.Id);

        // Act - Remove pet from order
        var removeOrderPetDto = new RemoveOrderPetDto
        {
            OrderId = testOrder.Id,
            PetId = testPet.Id
        };

        var orderAfterRemoval = await _orderService.RemoveOrderPetAsync(removeOrderPetDto);

        // Assert
        Assert.NotNull(orderAfterRemoval);
        Assert.Equal(testOrder.Id, orderAfterRemoval.Id);
        Assert.DoesNotContain(orderAfterRemoval.Pets, pet => pet.Id == testPet.Id);
    }

    [Fact]
    public async Task RemovePetFromOrderAsync_ShouldThrowException_WhenPetNotInOrder()
    {
        // Arrange
        var testCustomer = await CreateCustomerTestAsync();
        var testPet = await CreatePetTestAsync();
        var testOrder = await CreateOrderTestAsync(testCustomer.Id);

        var removeOrderPetDto = new RemoveOrderPetDto
        {
            OrderId = testOrder.Id,
            PetId = new Guid() // Pet not added to order
        };

        // Act & Assert
        await Assert.ThrowsAsync<AppException>(async () =>
            await _orderService.RemoveOrderPetAsync(removeOrderPetDto));

    }

    [Fact]
    public async Task GetOrdersByCustomerAsync_ShouldReturnOrdersForGivenCustomer()
    {
        // Arrange
        var testCustomer = await CreateCustomerTestAsync();
        var order1 = await CreateOrderTestAsync(testCustomer.Id);
        var order2 = await CreateOrderTestAsync(testCustomer.Id);

        // Act
        var customerOrders = await _orderService.GetOrdersByCustomer(testCustomer.Id);

        // Assert
        Assert.NotNull(customerOrders);
        Assert.True(customerOrders.Count >= 2);
        Assert.Contains(customerOrders, o => o.Id == order1.Id);
        Assert.Contains(customerOrders, o => o.Id == order2.Id);
    }

    [Fact]
    public async Task GetOrdersByCustomerAsync_ShouldThrowException_WhenCustomerDoesNotExist()
    {
        // Arrange
        var nonExistentCustomerId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<AppException>(async () =>
            await _orderService.GetOrdersByCustomer(nonExistentCustomerId));

    }

    [Fact]
    public async Task UpdateOrderAsync_ShouldUpdateOrderSuccessfully()
    {
        // Arrange
        var testCustomer = await CreateCustomerTestAsync();
        var testOrder = await CreateOrderTestAsync(testCustomer.Id);

        var updateOrderDto = new UpdateOrderDto
        {
            PickupDate = testOrder.PickupDate.AddDays(2),
            Status = OrderStatus.Open
        };

        // Act
        var updatedOrder = await _orderService.UpdateOrderAsync(testOrder.Id, updateOrderDto);

        // Assert
        Assert.NotNull(updatedOrder);
        Assert.Equal(testOrder.Id, updatedOrder.Id);
        Assert.Equal(updateOrderDto.PickupDate, updatedOrder.PickupDate);
        Assert.Equal(updateOrderDto.Status, updatedOrder.Status);
    }

    [Fact]
    public async Task UpdateOrderAsync_ShouldThrowException_WhenOrderDoesNotExist()
    {
        // Arrange
        var nonExistentOrderId = Guid.NewGuid();

        var updateOrderDto = new UpdateOrderDto
        {
            PickupDate = DateTime.UtcNow.AddDays(2),
            Status = OrderStatus.Processing
        };

        // Act & Assert
        await Assert.ThrowsAsync<AppException>(async () =>
            await _orderService.UpdateOrderAsync(nonExistentOrderId, updateOrderDto));

    }

    [Fact]
    public async Task UpdateOrderAsync_ShouldThrowException_WhenPickupDateIsInThePast()
    {
        // Arrange
        var testCustomer = await CreateCustomerTestAsync();
        var testOrder = await CreateOrderTestAsync(testCustomer.Id);

        var updateOrderDto = new UpdateOrderDto
        {
            PickupDate = DateTime.UtcNow.AddDays(-1), // Past date
            Status = OrderStatus.Open
        };

        // Act & Assert
        await Assert.ThrowsAsync<AppException>(async () =>
            await _orderService.UpdateOrderAsync(testOrder.Id, updateOrderDto));
    }

    [Fact]
    public async Task UpdateOrderAsync_ShouldThrowException_WhenSettingDeliveredWithNoPets()
    {
        // Arrange
        var testCustomer = await CreateCustomerTestAsync();
        var testOrder = await CreateOrderTestAsync(testCustomer.Id);

        var updateOrderDto = new UpdateOrderDto
        {
            Status = OrderStatus.Delivered
        };

        // Act & Assert
        await Assert.ThrowsAsync<AppException>(async () =>
            await _orderService.UpdateOrderAsync(testOrder.Id, updateOrderDto));
    }

    [Fact]
    public async Task UpdateOrderAsync_ShouldThrowException_WhenOrderIsAlreadyDelivered()
    {
        // Arrange
        var testCustomer = await CreateCustomerTestAsync();
        var testOrder = await CreateOrderTestAsync(testCustomer.Id);

        // First, update the order to Delivered with a pet
        var testPet = await CreatePetTestAsync();
        var createOrderPetDto = new CreateOrderPetDto
        {
            OrderId = testOrder.Id,
            PetId = testPet.Id
        };
        await _orderService.AddOrderPetAsync(createOrderPetDto);

        var deliveredUpdateDto = new UpdateOrderDto
        {
            Status = OrderStatus.Delivered
        };
        await _orderService.UpdateOrderAsync(testOrder.Id, deliveredUpdateDto);

        // Now, attempt to update the already Delivered order
        var updateOrderDto = new UpdateOrderDto
        {
            PickupDate = DateTime.UtcNow.AddDays(2),
            Status = OrderStatus.Processing
        };

        // Act & Assert
        await Assert.ThrowsAsync<AppException>(async () =>
            await _orderService.UpdateOrderAsync(testOrder.Id, updateOrderDto));

    }

    [Fact]
    public async Task UpdateOrderAsync_ShouldThrowException_WhenSettingProcessingWithPickupDate()
    {
        // Arrange
        var testCustomer = await CreateCustomerTestAsync();
        var testOrder = await CreateOrderTestAsync(testCustomer.Id);

        var updateOrderDto = new UpdateOrderDto
        {
            PickupDate = DateTime.UtcNow.AddDays(2),
            Status = OrderStatus.Processing
        };

        // Act & Assert
        await Assert.ThrowsAsync<AppException>(async () =>
            await _orderService.UpdateOrderAsync(testOrder.Id, updateOrderDto));
    }

    [Fact]
    public async Task RemovePetFromOrderAsync_ShouldThrowException_WhenOrderIsNotOpen()
    {
        // Arrange
        var testCustomer = await CreateCustomerTestAsync();
        var testPet = await CreatePetTestAsync();
        var testOrder = await CreateOrderTestAsync(testCustomer.Id);

        var createOrderPetDto = new CreateOrderPetDto
        {
            OrderId = testOrder.Id,
            PetId = testPet.Id
        };

        // Add pet to order first
        var updatedOrder = await _orderService.AddOrderPetAsync(createOrderPetDto);
        Assert.Contains(updatedOrder.Pets, pet => pet.Id == testPet.Id);

        // Update order status to Processing
        var updateOrderDto = new UpdateOrderDto
        {
            Status = OrderStatus.Processing
        };
        await _orderService.UpdateOrderAsync(testOrder.Id, updateOrderDto);

        var removeOrderPetDto = new RemoveOrderPetDto
        {
            OrderId = testOrder.Id,
            PetId = testPet.Id
        };

        // Act & Assert
        await Assert.ThrowsAsync<AppException>(async () =>
            await _orderService.RemoveOrderPetAsync(removeOrderPetDto));

    }

    [Fact]
    public async Task CreateOrderAsync_ShouldThrowException_WhenCustomerDoesNotExist()
    {
        // Arrange
        var newOrder = new CreateOrderDto()
        {
            CustomerId = Guid.NewGuid(), // Non-existent customer ID
            PickupDate = DateTime.UtcNow
        };

        // Act & Assert
        await Assert.ThrowsAsync<AppException>(async () =>
            await _orderService.CreateOrderAsync(newOrder));
    }

    [Fact]
    public async Task GetOrderAsync_ShouldThrowException_WhenOrderDoesNotExist()
    {
        // Arrange
        var nonExistentOrderId = Guid.NewGuid();
        // Act & Assert
        await Assert.ThrowsAsync<AppException>(async () =>
            await _orderService.GetOrderAsync(nonExistentOrderId));
    }


}