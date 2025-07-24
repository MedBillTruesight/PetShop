using Microsoft.AspNetCore.Mvc;
using Moq;
using PetShop.API.DTOs;
using PetShop.API.Services.Interfaces;
using PetShop.Controllers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace PetShop.Tests.Controllers
{
    public class OrdersControllerTests
    {
        private readonly Mock<IOrderService> _mockOrderService;
        private readonly OrdersController _controller;

        public OrdersControllerTests()
        {
            _mockOrderService = new Mock<IOrderService>();
            _controller = new OrdersController(_mockOrderService.Object);
        }

        [Fact]
        public async Task GetById_ReturnsOk_WhenOrderFound()
        {
            var orderId = Guid.NewGuid();
            var orderDto = new OrderDto
            {
                Id = orderId,
                PickupDate = DateTime.UtcNow.AddDays(1),
                Status = "Open",
                Pets = new List<PetDto>
                {
                    new PetDto
                    {
                        Id = Guid.NewGuid(),
                        Name = "Fluffy",
                        Kind = "Cat",
                        Color = "White",
                        Price = 150
                    }
                },
                EstimatedCost = 150,
                ActualCost = null,
                Customer = new CustomerDto
                {
                    Id = Guid.NewGuid(),
                    FullName = "John Doe",
                    Email = "john@example.com",
                    PhoneNumber = "123456789"
                }
            };

            _mockOrderService.Setup(s => s.GetByIdAsync(orderId)).ReturnsAsync(orderDto);
            
            var result = await _controller.GetById(orderId);
            
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(orderDto, okResult.Value);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenOrderNotFound()
        {
            var orderId = Guid.NewGuid();
            _mockOrderService.Setup(s => s.GetByIdAsync(orderId)).ReturnsAsync((OrderDto?)null);
            
            var result = await _controller.GetById(orderId);
            
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtAction()
        {
            var createDto = new CreateOrderDto
            {
                CustomerId = Guid.NewGuid(),
                PickupDate = DateTime.UtcNow.AddDays(2),
                PetIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }
            };

            var orderDto = new OrderDto
            {
                Id = Guid.NewGuid(),
                PickupDate = createDto.PickupDate,
                Status = "Open",
                Pets = new List<PetDto>
                {
                    new PetDto { Id = createDto.PetIds[0], Name = "Buddy", Kind = "Dog", Color = "Brown", Price = 200 },
                    new PetDto { Id = createDto.PetIds[1], Name = "Mittens", Kind = "Cat", Color = "Gray", Price = 180 }
                },
                EstimatedCost = 380.00m,
                ActualCost = null,
                Customer = new CustomerDto
                {
                    Id = createDto.CustomerId,
                    FullName = "Jane Smith",
                    Email = "jane@example.com",
                    PhoneNumber = "987654321"
                }
            };

            _mockOrderService.Setup(s => s.CreateAsync(createDto)).ReturnsAsync(orderDto);
            
            var result = await _controller.Create(createDto);
            
            var createdAt = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal("GetById", createdAt.ActionName);
            Assert.Equal(orderDto.Id, ((OrderDto)createdAt.Value!).Id);
        }

        [Fact]
        public async Task Update_ReturnsNoContent_WhenSuccessful()
        {
            var orderId = Guid.NewGuid();
            var updateDto = new UpdateOrderDto
            {
                PickupDate = DateTime.UtcNow.AddDays(3),
                Status = "Processing",
                PetIds = new List<Guid> { Guid.NewGuid() }
            };

            _mockOrderService.Setup(s => s.UpdateAsync(orderId, updateDto)).ReturnsAsync(true);

            var result = await _controller.Update(orderId, updateDto);
            
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenFailed()
        {
            var orderId = Guid.NewGuid();
            var updateDto = new UpdateOrderDto
            {
                PickupDate = DateTime.UtcNow.AddDays(5),
                Status = "Delivered",
                PetIds = new List<Guid> { Guid.NewGuid() }
            };

            _mockOrderService.Setup(s => s.UpdateAsync(orderId, updateDto)).ReturnsAsync(false);
            
            var result = await _controller.Update(orderId, updateDto);
            
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
