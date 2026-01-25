using PetShop.Application.DTOs;
using PetShop.Application.Interfaces.Mappers;
using PetShop.Application.Interfaces.Services;
using PetShop.Domain.Entities;
using PetShop.Domain.Enums;
using PetShop.Domain.Exceptions;
using PetShop.Domain.Interfaces.Repositories;

namespace PetShop.Application.Features.Orders;
public class OrderService : IOrderService
{
	private readonly IOrderRepository _repository;
	private readonly ICustomerService _customerService;
	private readonly IPetService _petService;
	private readonly IOrderMapper _mapper;
	public OrderService(IOrderRepository repository, IOrderMapper mapper, ICustomerService customerService, IPetService petService)
	{
		_repository = repository;
		_mapper = mapper;
		_customerService = customerService;
		_petService = petService;
	}

	public async Task<OrderDto> AddOrderPetAsync(CreateOrderPetDto createOrderPetDto)
	{
		var existingOrder = await _repository.GetOrderByIdAsync(createOrderPetDto.OrderId);
		if (existingOrder == null)
		{
			throw new AppException($"The Order with that ID {createOrderPetDto.OrderId} does not exist.");
		}

		if (existingOrder.Status != OrderStatus.Open)
		{
			throw new AppException($"Cannot add pet to order with ID {createOrderPetDto.OrderId} because it is not open.");
		}

		var existingOrderPet = existingOrder.OrderPets.FirstOrDefault(op => op.PetId == createOrderPetDto.PetId);
		if (existingOrderPet != null)
		{
			throw new AppException($"Pet with ID {createOrderPetDto.PetId} is already added to the order.");
		}

		var existingPet = await _petService.GetPetAsync(createOrderPetDto.PetId);

		if (existingPet == null)
		{
			throw new AppException($"Pet with ID {createOrderPetDto.PetId} does not exist.");
		}

		var orderPet = _mapper.ToDomain(createOrderPetDto);
		await _repository.AddOrderPetAsync(orderPet);

		var updatedOrder = await _repository.GetOrderByIdAsync(existingOrder.Id);
		if (updatedOrder == null)
		{
			throw new Exception($"Failed to retrieve updated order with ID {existingOrder.Id} after adding pet.");
		}
		return _mapper.ToDto(updatedOrder);
	}

	public async Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto)
	{
		var customer = await _customerService.GetCustomerAsync(createOrderDto.CustomerId);
		if (customer == null)
		{
			throw new AppException($"Customer does not exist.");
		}

		if (createOrderDto.PickupDate < DateTime.UtcNow.Date)
		{
			throw new AppException("Pickup date must be today or sometime in the future.");
		}

		Order order = _mapper.ToDomain(createOrderDto);
		order.Status = OrderStatus.Open;

		await _repository.CreateOrderAsync(order);

		return _mapper.ToDto(order);
	}

	public async Task<List<OrderDto>> GetAllOrdersAsync()
	{
		var orders = await _repository.GetAllOrdersAsync();
		var orderDtos = orders.Select(_mapper.ToDto).ToList();

		return orderDtos;
	}

	public async Task<OrderDto?> GetOrderAsync(Guid id)
	{
		var order = await _repository.GetOrderByIdAsync(id);

		if (order == null) throw new AppException("Order not found.");

		var customer = await _customerService.GetCustomerAsync(order.CustomerId);

		var orderDto = _mapper.ToDto(order);
		orderDto.CustomerName = customer?.FirstName + " " + customer?.LastName;

		if (orderDto.Status == OrderStatus.Delivered)
		{
			orderDto.Cost = order?.ActualCost ?? 0;
			orderDto.EstimatedCost = order?.ActualCost ?? 0;
		}
		else
		{
			orderDto.Cost = order.OrderPets.Sum(op => op.Pet.Price);
			orderDto.EstimatedCost = order.OrderPets.Sum(op => op.Pet.Price);
		}
		return orderDto;
	}

	public async Task<List<OrderDto>> GetOrdersByCustomer(Guid customerId)
	{
		var customer = await _customerService.GetCustomerAsync(customerId);
		if (customer == null)
		{
			throw new AppException($"Customer with the ID {customerId} does not exist.");
		}

		var orders = await _repository.GetOrdersByCustomerAsync(customerId);
		return orders.Select(_mapper.ToDto).ToList();
	}

	public async Task<OrderDto?> RemoveOrderPetAsync(RemoveOrderPetDto removeOrderPetDto)
	{
		var existingOrder = await _repository.GetOrderByIdAsync(removeOrderPetDto.OrderId);
		if (existingOrder == null)
		{
			throw new AppException($"Order with the ID {removeOrderPetDto.OrderId} does not exist.");
		}

		var existingOrderPet = existingOrder.OrderPets.FirstOrDefault(op => op.PetId == removeOrderPetDto.PetId);
		if (existingOrderPet == null)
		{
			throw new AppException($"Pet with the ID {removeOrderPetDto.PetId} is not in the order.");
		}

		if (existingOrder.Status != OrderStatus.Open)
		{
			throw new AppException($"Cannot remove pet from order with ID {removeOrderPetDto.OrderId} because it is not open.");
		}

		await _repository.RemoveOrderPetAsync(existingOrderPet);
		return _mapper.ToDto(existingOrder);
	}

	public async Task<OrderDto?> UpdateOrderAsync(Guid id, UpdateOrderDto updateOrderDto)
	{
		var existingOrder = await _repository.GetOrderByIdAsync(id);

		if (existingOrder == null)
		{
			throw new AppException($"Order with the ID {id} does not exist.");
		}

		var customer = await _customerService.GetCustomerAsync(existingOrder.CustomerId);

		if (updateOrderDto.PickupDate.HasValue && updateOrderDto.PickupDate < DateTime.UtcNow.Date)
		{
			throw new AppException("Pickup date must be today or in the future.");
		}

		if (updateOrderDto.Status == OrderStatus.Processing)
		{
			if (updateOrderDto.PickupDate.HasValue)
			{
				throw new AppException("Cannot set pickup date when status is Processing.");
			}
		}

		if (existingOrder.Status == OrderStatus.Delivered)
		{
			throw new AppException($"Cannot update order with ID {id} because it is Delivered.");
		}

		if (updateOrderDto.Status == OrderStatus.Delivered && existingOrder.OrderPets.Count == 0)
		{
			throw new AppException($"Cannot set order with the ID {id} to Delivered because it has no pets.");
		}

		existingOrder.PickupDate = updateOrderDto.PickupDate ?? existingOrder.PickupDate;
		existingOrder.Status = updateOrderDto.Status ?? existingOrder.Status;

		// If order is being marked as Delivered, calculate ActualCost
		if (existingOrder.Status == OrderStatus.Delivered)
		{
			existingOrder.ActualCost = existingOrder.OrderPets.Sum(op => op.Pet.Price);
		}

		await _repository.UpdateOrderAsync(existingOrder);
		OrderDto orderDto = _mapper.ToDto(existingOrder);

		orderDto.CustomerName = customer?.FirstName + " " + customer?.LastName;

		if (orderDto.Status == OrderStatus.Delivered)
		{
			orderDto.Cost = existingOrder?.ActualCost ?? 0;
			orderDto.EstimatedCost = existingOrder?.ActualCost ?? 0;
		}

		return orderDto;
	}
}