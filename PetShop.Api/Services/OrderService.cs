using Microsoft.EntityFrameworkCore;
using PetShop.Api.Dtos;
using PetShop.Api.Enums;
using PetShop.Api.Mapper;
using PetShop.Api.Models;
using PetShop.Api.Repository;

namespace PetShop.Api.Services
{
    public class OrderService: IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICustomerRepository _customerRepository;

        public OrderService(IOrderRepository orderRepository, ICustomerRepository customerRepository)
        {
            _orderRepository = orderRepository;
            _customerRepository = customerRepository;
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto)
        {
            var customer = await _customerRepository.GetById(dto.CustomerId);
            if (customer == null)
                throw new InvalidOperationException("Customer not found.");

            var order = dto.ToModel();

            return (await _orderRepository.AddAsync(order)).ToDto();
        }

        public async Task<OrderDto?> GetOrderAsync(Guid orderId)
        {
            var order = await _orderRepository.GetById(orderId);
            if (order == null) return null;

            var orderDto = order.ToDto();

            if (order.Status == OrderStatus.Delivered)
            {
                orderDto.ActualCost = order.ActualCost ?? 0;
                orderDto.EstimatedCost = null; 
            }
            else
            {
                orderDto.EstimatedCost = order.Pets.Sum(p => p.Price);
                orderDto.ActualCost = null;
            }

            return orderDto;
        }


        public async Task<OrderDto> MarkAsDeliveredAsync(Guid orderId)
        {
            var order = await _orderRepository.GetById(orderId);
            if (order == null)
                throw new KeyNotFoundException("Order not found");

            if (order.Status != OrderStatus.Processing)
                throw new InvalidOperationException("Only orders in Processing can be marked as Delivered.");

            if (!order.Pets.Any())
                throw new InvalidOperationException("Cannot deliver an order with no pets.");

            order.ActualCost = order.Pets.Sum(p => p.Price);

            order.Status = OrderStatus.Delivered;

            await _orderRepository.Update(order);

            return order.ToDto();
        }

        public async Task<OrderDto> UpdateOrderAsync(Guid orderId, UpdateOrderDto dto)
        {
            var order = await _orderRepository.GetById(orderId);
            if (order == null)
                throw new KeyNotFoundException("Order not found");

            if (dto.Status.HasValue && !Enum.IsDefined(typeof(OrderStatus), dto.Status.Value))
            {
                throw new InvalidOperationException($"Invalid order status: {dto.Status}");
            }

            switch (order.Status)
            {
                case OrderStatus.Open:
                    if (dto.Pets != null)
                    {
                        var dtoPetNames = dto.Pets.Select(p => p.Name).ToHashSet();
                        var petsToRemove = order.Pets
                            .Where(p => !dtoPetNames.Contains(p.Name))
                            .ToList();

                        foreach (var pet in petsToRemove)
                            _orderRepository.RemovePet(pet.Id);

                        foreach (var petDto in dto.Pets)
                        {
                            var existingPet = order.Pets.FirstOrDefault(p => p.Name == petDto.Name);
                            if (existingPet == null)
                            {
                                var newPet = petDto.ToModel(null);
                                newPet.Id = petDto.Id;
                                order.Pets.Add(newPet);
                                await _orderRepository.AddPetAsync(newPet);
                            }
                            else
                            {
                                existingPet.Kind = petDto.Kind;
                                existingPet.Color = petDto.Color;
                                existingPet.Breed = petDto.Breed;
                                existingPet.Price = petDto.Price;
                            }
                        }
                    }

                    if (dto.Status == OrderStatus.Processing)
                    {
                        if (!order.Pets.Any())
                            throw new InvalidOperationException("At least one pet is required to move order to Processing");

                        order.Status = OrderStatus.Processing;
                    }
                    else if (dto.Status == OrderStatus.Delivered)
                    {
                        throw new InvalidOperationException("Cannot move directly from Open to Delivered");
                    }
                    break;

                case OrderStatus.Processing:
                    if (dto.PickupDate.HasValue)
                        order.PickupDate = dto.PickupDate.Value;

                    if (dto.Status == OrderStatus.Delivered)
                    {
                        // move forward to delivered
                        order.Status = OrderStatus.Delivered;
                    }
                    else if (dto.Status.HasValue && dto.Status != OrderStatus.Processing)
                    {
                        throw new InvalidOperationException("Cannot move from Processing to this status");
                    }
                    break;

                case OrderStatus.Delivered:
                    throw new InvalidOperationException("Delivered orders cannot be updated");

                default:
                    throw new InvalidOperationException("Unsupported order status");
            }


            await _orderRepository.Update(order);
            return order.ToDto();
        }

    }
}
