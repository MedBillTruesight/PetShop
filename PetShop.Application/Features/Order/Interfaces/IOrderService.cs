using PetShop.Application.DTOs;

namespace PetShop.Application.Features.Order.Interfaces;

public interface IOrderService
{
	Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto);
	Task<OrderDto> AddOrderPetAsync(CreateOrderPetDto createOrderPetDto);
	Task<OrderDto?> GetOrderAsync(int id);
	Task<List<OrderDto>> GetAllOrdersAsync();
	Task<List<OrderDto>> GetOrdersByCustomer(int customerId);
	Task<OrderDto?> UpdateOrderAsync(int id, UpdateOrderDto updateOrderDto);
	Task<OrderDto?> RemoveOrderPetAsync(RemoveOrderPetDto removeOrderPetDto);
}
