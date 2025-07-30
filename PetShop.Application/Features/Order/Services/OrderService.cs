
using PetShop.Application.DTOs;
using PetShop.Application.Features.Customer.Interfaces;
using PetShop.Application.Features.Order.Interfaces;
using PetShop.Application.Features.Pet.Interfaces;
using PetShop.Application.Mappers;
using PetShop.Domain.Interfaces;

namespace PetShop.Application.Features.Order.Services;

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
			throw new ArgumentException($"Order with ID {createOrderPetDto.OrderId} does not exist.");
		}

		if (existingOrder.Status != Domain.Enums.OrderStatus.Open)
		{
			throw new ArgumentException($"Cannot add pet to order with ID {createOrderPetDto.OrderId} because it is not open.");
		}

		var existingOrderPet = existingOrder.OrderPets.FirstOrDefault(op => op.PetId == createOrderPetDto.PetId);
		if (existingOrderPet != null)
		{
			throw new ArgumentException($"Pet with ID {createOrderPetDto.PetId} is already added to the order.");
		}

		var existingPet = await _petService.GetPetAsync(createOrderPetDto.PetId);

		if (existingPet == null)
		{
			throw new ArgumentException($"Pet with ID {createOrderPetDto.PetId} does not exist.");
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
			throw new ArgumentException($"Customer with ID {createOrderDto.CustomerId} does not exist.");
		}

		if (createOrderDto.PickupDate < DateTime.UtcNow.Date)
		{
			throw new ArgumentException("Pickup date must be today or in the future.");
		}

		Domain.Entities.Order order = _mapper.ToDomain(createOrderDto);
		order.Status = Domain.Enums.OrderStatus.Open;

		await _repository.CreateOrderAsync(order);

		return _mapper.ToDto(order);
	}

	public async Task<List<OrderDto>> GetAllOrdersAsync()
	{
		var orders = await _repository.GetAllOrdersAsync();
		return orders.Select(_mapper.ToDto).ToList();
	}

	public async Task<OrderDto?> GetOrderAsync(int id)
	{
		var order = await _repository.GetOrderByIdAsync(id);
		if (order == null) return null;

		OrderDto orderDto = _mapper.ToDto(order);
		if (orderDto.Status == Domain.Enums.OrderStatus.Delivered)
		{
			orderDto.Cost = order.ActualCost ?? 0;
			orderDto.EstimatedCost = order.ActualCost ?? 0;
		}
		else
		{
			orderDto.Cost = order.OrderPets.Sum(op => op.Pet.Price);
			orderDto.EstimatedCost = order.OrderPets.Sum(op => op.Pet.Price);
		}
		return orderDto;
	}

	public async Task<List<OrderDto>> GetOrdersByCustomer(int customerId)
	{
		var customer = await _customerService.GetCustomerAsync(customerId);
		if (customer == null)
		{
			throw new ArgumentException($"Customer with ID {customerId} does not exist.");
		}

		var orders = await _repository.GetOrdersByCustomerAsync(customerId);
		return orders.Select(_mapper.ToDto).ToList();
	}

	public async Task<OrderDto?> RemoveOrderPetAsync(RemoveOrderPetDto removeOrderPetDto)
	{
		var existingOrder = await _repository.GetOrderByIdAsync(removeOrderPetDto.OrderId);
		if (existingOrder == null)
		{
			throw new ArgumentException($"Order with ID {removeOrderPetDto.OrderId} does not exist.");
		}

		var existingOrderPet = existingOrder.OrderPets.FirstOrDefault(op => op.PetId == removeOrderPetDto.PetId);
		if (existingOrderPet == null)
		{
			throw new ArgumentException($"Pet with ID {removeOrderPetDto.PetId} is not in the order.");
		}

		if (existingOrder.Status != Domain.Enums.OrderStatus.Open)
		{
			throw new ArgumentException($"Cannot remove pet from order with ID {removeOrderPetDto.OrderId} because it is not open.");
		}

		await _repository.RemoveOrderPetAsync(existingOrderPet);
		return _mapper.ToDto(existingOrder);
	}

	public async Task<OrderDto?> UpdateOrderAsync(int id, UpdateOrderDto updateOrderDto)
	{
		var existingOrder = await _repository.GetOrderByIdAsync(id);
		if (existingOrder == null)
		{
			throw new ArgumentException($"Order with ID {id} does not exist.");
		}

		if (updateOrderDto.PickupDate.HasValue && updateOrderDto.PickupDate < DateTime.UtcNow.Date)
		{
			throw new ArgumentException("Pickup date must be today or in the future.");
		}

		if (updateOrderDto.Status == Domain.Enums.OrderStatus.Delivered)
		{
			if (updateOrderDto.PickupDate.HasValue)
			{
				throw new ArgumentException("Cannot set pickup date when status is Processing.");
			}
		}

		if (existingOrder.Status == Domain.Enums.OrderStatus.Delivered)
		{
			throw new ArgumentException($"Cannot update order with ID {id} because it is Delivered.");
		}

		if (updateOrderDto.Status == Domain.Enums.OrderStatus.Delivered && existingOrder.OrderPets.Count == 0)
		{
			throw new ArgumentException($"Cannot set order with ID {id} to Delivered because it has no pets.");
		}

		existingOrder.PickupDate = updateOrderDto.PickupDate ?? existingOrder.PickupDate;
		existingOrder.Status = updateOrderDto.Status ?? existingOrder.Status;

		if (existingOrder.Status == Domain.Enums.OrderStatus.Delivered)
		{
			existingOrder.ActualCost = existingOrder.OrderPets.Sum(op => op.Pet.Price);
		}

		await _repository.UpdateOrderAsync(existingOrder);
		OrderDto orderDto = _mapper.ToDto(existingOrder);

		if (orderDto.Status == Domain.Enums.OrderStatus.Delivered)
		{
			orderDto.Cost = existingOrder.ActualCost ?? 0;
			orderDto.EstimatedCost = existingOrder.ActualCost ?? 0;
		}

		return orderDto;
	}
}