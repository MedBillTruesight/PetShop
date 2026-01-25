using PetShop.Application.DTOs;

namespace PetShop.Application.Interfaces.Services;
public interface IOrderService
{
	Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto);
	Task<OrderDto> AddOrderPetAsync(CreateOrderPetDto createOrderPetDto);
	Task<OrderDto?> GetOrderAsync(Guid id);
	Task<List<OrderDto>> GetAllOrdersAsync();
	Task<List<OrderDto>> GetOrdersByCustomer(Guid customerId);
	Task<OrderDto?> UpdateOrderAsync(Guid id, UpdateOrderDto updateOrderDto);
	Task<OrderDto?> RemoveOrderPetAsync(RemoveOrderPetDto removeOrderPetDto);
}