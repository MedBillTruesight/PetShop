using FakeItEasy;
using FluentAssertions;
using PetShop.Api.Dtos;
using PetShop.Api.Enums;
using PetShop.Api.Models;
using PetShop.Api.Repository;
using PetShop.Api.Services;

namespace PetShop.Tests
{


    public class OrderServiceTests
    {
        private readonly IOrderRepository _orderRepo;
        private readonly ICustomerRepository _customerRepo;
        private readonly OrderService _sut;

        public OrderServiceTests()
        {
            _orderRepo = A.Fake<IOrderRepository>();
            _customerRepo = A.Fake<ICustomerRepository>();
            _sut = new OrderService(_orderRepo, _customerRepo);
        }

        #region CreateOrderAsync

        [Fact]
        public async Task CreateOrderAsync_ShouldThrow_WhenCustomerNotFound()
        {
            // Arrange
            var dto = new CreateOrderDto { CustomerId = Guid.NewGuid() };
            A.CallTo(() => _customerRepo.GetById(dto.CustomerId))
                .Returns(Task.FromResult<Customer?>(null));

            // Act
            var act = async () => await _sut.CreateOrderAsync(dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Customer not found.");
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldReturnOrder_WhenCustomerExists()
        {
            var dto = new CreateOrderDto { CustomerId = Guid.NewGuid() };
            var customer = new Customer { Id = dto.CustomerId };
            var order = new Order { Id = Guid.NewGuid() };

            A.CallTo(() => _customerRepo.GetById(dto.CustomerId))
                .Returns(customer);
            A.CallTo(() => _orderRepo.AddAsync(A<Order>._))
                .Returns(order);

            var result = await _sut.CreateOrderAsync(dto);

            result.Should().NotBeNull();
            result.Id.Should().Be(order.Id);
            A.CallTo(() => _orderRepo.AddAsync(A<Order>._))
                .MustHaveHappenedOnceExactly();
        }

        #endregion

        #region GetOrderAsync

        [Fact]
        public async Task GetOrderAsync_ShouldReturnNull_WhenOrderNotFound()
        {
            A.CallTo(() => _orderRepo.GetById(A<Guid>._))
                .Returns(Task.FromResult<Order?>(null));

            var result = await _sut.GetOrderAsync(Guid.NewGuid());

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetOrderAsync_ShouldSetEstimatedCost_WhenOpen()
        {
            var order = new Order
            {
                Status = OrderStatus.Open,
                Pets = new List<Pet> { new Pet { Price = 50 }, new Pet { Price = 70 } }
            };

            A.CallTo(() => _orderRepo.GetById(A<Guid>._))
                .Returns(order);

            var result = await _sut.GetOrderAsync(Guid.NewGuid());

            result.EstimatedCost.Should().Be(120);
            result.ActualCost.Should().BeNull();
        }

        [Fact]
        public async Task GetOrderAsync_ShouldSetActualCost_WhenDelivered()
        {
            var order = new Order
            {
                Status = OrderStatus.Delivered,
                ActualCost = 200,
                Pets = new List<Pet> { new Pet { Price = 100 } }
            };

            A.CallTo(() => _orderRepo.GetById(A<Guid>._))
                .Returns(order);

            var result = await _sut.GetOrderAsync(Guid.NewGuid());

            result.ActualCost.Should().Be(200);
            result.EstimatedCost.Should().BeNull();
        }

        #endregion

        #region MarkAsDeliveredAsync

        [Fact]
        public async Task MarkAsDeliveredAsync_ShouldThrow_WhenOrderNotFound()
        {
            A.CallTo(() => _orderRepo.GetById(A<Guid>._))
                .Returns(Task.FromResult<Order?>(null));

            var act = async () => await _sut.MarkAsDeliveredAsync(Guid.NewGuid());

            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("Order not found");
        }

        [Fact]
        public async Task MarkAsDeliveredAsync_ShouldThrow_WhenOrderNotProcessing()
        {
            var order = new Order { Status = OrderStatus.Open };
            A.CallTo(() => _orderRepo.GetById(A<Guid>._))
                .Returns(order);

            var act = async () => await _sut.MarkAsDeliveredAsync(Guid.NewGuid());

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Only orders in Processing can be marked as Delivered.");
        }

        [Fact]
        public async Task MarkAsDeliveredAsync_ShouldThrow_WhenNoPets()
        {
            var order = new Order { Status = OrderStatus.Processing, Pets = new List<Pet>() };
            A.CallTo(() => _orderRepo.GetById(A<Guid>._))
                .Returns(order);

            var act = async () => await _sut.MarkAsDeliveredAsync(Guid.NewGuid());

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Cannot deliver an order with no pets.");
        }

        [Fact]
        public async Task MarkAsDeliveredAsync_ShouldUpdateToDelivered()
        {
            var pets = new List<Pet> { new Pet { Price = 30 }, new Pet { Price = 70 } };
            var order = new Order { Status = OrderStatus.Processing, Pets = pets };

            A.CallTo(() => _orderRepo.GetById(A<Guid>._))
                .Returns(order);

            var result = await _sut.MarkAsDeliveredAsync(Guid.NewGuid());

            result.Status.Should().Be(OrderStatus.Delivered.ToString());
            result.ActualCost.Should().Be(100);
            A.CallTo(() => _orderRepo.Update(A<Order>.That.Matches(o => o.Status == OrderStatus.Delivered)))
                .MustHaveHappenedOnceExactly();
        }

        #endregion

        #region UpdateOrderAsync

        [Fact]
        public async Task UpdateOrderAsync_ShouldThrow_WhenOrderNotFound()
        {
            A.CallTo(() => _orderRepo.GetById(A<Guid>._))
                .Returns(Task.FromResult<Order?>(null));

            var act = async () => await _sut.UpdateOrderAsync(Guid.NewGuid(), new UpdateOrderDto());

            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("Order not found");
        }

        [Fact]
        public async Task UpdateOrderAsync_ShouldThrow_WhenInvalidStatus()
        {
            var order = new Order { Status = OrderStatus.Open };
            A.CallTo(() => _orderRepo.GetById(A<Guid>._)).Returns(order);

            var dto = new UpdateOrderDto { Status = (OrderStatus)999 };

            var act = async () => await _sut.UpdateOrderAsync(Guid.NewGuid(), dto);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Invalid order status: 999");
        }

        [Fact]
        public async Task UpdateOrderAsync_ShouldAddPet_WhenOrderIsOpen()
        {
            var order = new Order { Status = OrderStatus.Open, Pets = new List<Pet>() };
            A.CallTo(() => _orderRepo.GetById(A<Guid>._)).Returns(order);

            var dto = new UpdateOrderDto
            {
                Pets = new List<UpdatePetDto> { new UpdatePetDto { Id = Guid.NewGuid(), Name = "Dog", Price = 100 } }
            };

            var result = await _sut.UpdateOrderAsync(Guid.NewGuid(), dto);

            result.Pets.Should().ContainSingle(p => p.Name == "Dog");
            A.CallTo(() => _orderRepo.AddPetAsync(A<Pet>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task UpdateOrderAsync_ShouldThrow_WhenOpenToDelivered()
        {
            var order = new Order { Status = OrderStatus.Open };
            A.CallTo(() => _orderRepo.GetById(A<Guid>._)).Returns(order);

            var dto = new UpdateOrderDto { Status = OrderStatus.Delivered };

            var act = async () => await _sut.UpdateOrderAsync(Guid.NewGuid(), dto);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Cannot move directly from Open to Delivered");
        }

        [Fact]
        public async Task UpdateOrderAsync_ShouldThrow_WhenDeliveredUpdated()
        {
            var order = new Order { Status = OrderStatus.Delivered };
            A.CallTo(() => _orderRepo.GetById(A<Guid>._)).Returns(order);

            var act = async () => await _sut.UpdateOrderAsync(Guid.NewGuid(), new UpdateOrderDto());

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Delivered orders cannot be updated");
        }

        [Fact]
        public async Task UpdateOrderAsync_ShouldMoveProcessingToDelivered()
        {
            var order = new Order { Status = OrderStatus.Processing };
            A.CallTo(() => _orderRepo.GetById(A<Guid>._)).Returns(order);

            var dto = new UpdateOrderDto { Status = OrderStatus.Delivered };

            var result = await _sut.UpdateOrderAsync(Guid.NewGuid(), dto);

            result.Status.Should().Be(OrderStatus.Delivered.ToString());
            A.CallTo(() => _orderRepo.Update(A<Order>.That.Matches(o => o.Status == OrderStatus.Delivered)))
                .MustHaveHappenedOnceExactly();
        }

        #endregion
    }
}
