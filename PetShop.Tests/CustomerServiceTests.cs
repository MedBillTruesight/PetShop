using FakeItEasy;
using FluentAssertions;
using PetShop.Api.Dtos;
using PetShop.Api.Enums;
using PetShop.Api.Models;
using PetShop.Api.Repository;
using PetShop.Api.Services;

namespace PetShop.Tests
{
    public class CustomerServiceTests
    {
        private readonly ICustomerRepository _customerRepo;
        private readonly IOrderRepository _orderRepo;
        private readonly CustomerService _sut;

        public CustomerServiceTests()
        {
            _customerRepo = A.Fake<ICustomerRepository>();
            _orderRepo = A.Fake<IOrderRepository>();
            _sut = new CustomerService(_customerRepo, _orderRepo);
        }

        #region CreateCustomerAsync

        [Fact]
        public async Task CreateCustomerAsync_ShouldReturnCreatedCustomer()
        {
            var request = new CreateCustomerDto { FirstName = "Pet", LastName = "Petter" };
            var customer = new Customer { Id = Guid.NewGuid(), FirstName = "Pet", LastName = "Petter" };

            A.CallTo(() => _customerRepo.AddAsync(A<Customer>._))
                .Returns(customer);

            var result = await _sut.CreateCustomerAsync(request);

            result.Should().NotBeNull();
            result.FirstName.Should().Be("Pet");
            result.LastName.Should().Be("Petter");
            A.CallTo(() => _customerRepo.AddAsync(A<Customer>._)).MustHaveHappenedOnceExactly();
        }

        #endregion

        #region GetCustomerAsync

        [Fact]
        public async Task GetCustomerAsync_ShouldReturnNull_WhenCustomerNotFound()
        {
            A.CallTo(() => _customerRepo.GetById(A<Guid>._))
                .Returns(Task.FromResult<Customer?>(null));

            var result = await _sut.GetCustomerAsync(Guid.NewGuid());

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetCustomerAsync_ShouldReturnCustomerWithDues()
        {
            var customerId = Guid.NewGuid();
            var customer = new Customer { Id = customerId, FirstName = "Kips" };

            var orders = new List<Order>
        {
            new Order
            {
                Status = OrderStatus.Open,
                Pets = new List<Pet> { new Pet { Price = 50 }, new Pet { Price = 100 } }
            },
            new Order
            {
                Status = OrderStatus.Delivered,
                ActualCost = 200,
                Pets = new List<Pet> { new Pet { Price = 300 } }
            }
        };

            A.CallTo(() => _customerRepo.GetById(customerId)).Returns(customer);
            A.CallTo(() => _orderRepo.GetByCustomerId(customerId)).Returns(orders);

            var result = await _sut.GetCustomerAsync(customerId);

            result.Should().NotBeNull();
            result.Id.Should().Be(customerId);
            result.EstimatedDue.Should().Be(150); 
            result.ActualDue.Should().Be(200); 
        }

        [Fact]
        public async Task GetCustomerAsync_ShouldHandleNoOrders()
        {
            var customerId = Guid.NewGuid();
            var customer = new Customer { Id = customerId };

            A.CallTo(() => _customerRepo.GetById(customerId)).Returns(customer);
            A.CallTo(() => _orderRepo.GetByCustomerId(customerId)).Returns(new List<Order>());

            var result = await _sut.GetCustomerAsync(customerId);

            result.EstimatedDue.Should().Be(0);
            result.ActualDue.Should().Be(0);
        }

        #endregion

        #region UpdateCustomerAsync

        [Fact]
        public async Task UpdateCustomerAsync_ShouldReturnNull_WhenCustomerNotFound()
        {
            A.CallTo(() => _customerRepo.GetById(A<Guid>._))
                .Returns(Task.FromResult<Customer?>(null));

            var request = new UpdateCustomerDto { FirstName = "Updated",LastName= "Kips" };
            var result = await _sut.UpdateCustomerAsync(Guid.NewGuid(), request);

            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateCustomerAsync_ShouldUpdateAndReturnCustomer()
        {
            var customerId = Guid.NewGuid();
            var customer = new Customer { Id = customerId, FirstName = "Old" };
            var updatedCustomer = new Customer { Id = customerId, FirstName = "Updated", LastName = "Kips" };

            A.CallTo(() => _customerRepo.GetById(customerId)).Returns(customer);
            A.CallTo(() => _customerRepo.Update(A<Customer>._)).Returns(updatedCustomer);

            var request = new UpdateCustomerDto { FirstName = "Updated", LastName = "Kips" };

            var result = await _sut.UpdateCustomerAsync(customerId, request);

            result.Should().NotBeNull();
            result.FirstName.Should().Be("Updated");
            result.LastName.Should().Be("Kips");
            A.CallTo(() => _customerRepo.Update(A<Customer>.That.Matches(c => c.FirstName == "Updated")))
                .MustHaveHappenedOnceExactly();
        }

        #endregion
    }
}
