using PetShop.Api.Dtos;
using PetShop.Api.Enums;
using PetShop.Api.Mapper;
using PetShop.Api.Models;
using PetShop.Api.Repository;

namespace PetShop.Api.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IOrderRepository _orderRepository;
        public CustomerService(ICustomerRepository customerRepository,
                         IOrderRepository orderRepository)
        {
            _customerRepository = customerRepository;
            _orderRepository = orderRepository;
        }
        public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto request)
        {

            Customer customer = request.ToModel();
            var createdCustomer = await _customerRepository.AddAsync(customer);

            return createdCustomer.ToDto();

        }

        public async Task<CustomerDto> GetCustomerAsync(Guid id)
        {
            var customer = await _customerRepository.GetById(id);
            if (customer == null) return null;

            var orders = await _orderRepository.GetByCustomerId(id);

            var customerDto = customer.ToDto();

            customerDto.EstimatedDue = orders
                .Where(o => o.Status != OrderStatus.Delivered)
                .Sum(o => o.Pets.Sum(p => p.Price));

            customerDto.ActualDue = orders.Where(o => o.Status == OrderStatus.Delivered)
                .Sum(o => o.ActualCost ?? 0);

            return customerDto;
        }

        public async Task<CustomerDto> UpdateCustomerAsync(Guid id, UpdateCustomerDto request)
        {

            var customer = await _customerRepository.GetById(id);
            if (customer == null) return null;

            customer.FirstName = request.FirstName;
            customer.LastName = request.LastName;
            customer.Email = request.Email;
            customer.Phone = request.Phone;
            customer.Address = request.Address;

            var updatedCustomer = await _customerRepository.Update(customer);

            return updatedCustomer.ToDto();

        }
    }
}
