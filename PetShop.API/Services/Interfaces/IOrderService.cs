using PetShop.API.DTOs;

namespace PetShop.API.Services.Interfaces;

public interface IOrderService
{
    Task<OrderDto?> GetByIdAsync(Guid id);
    Task<List<OrderDto>> GetAllAsync();
    Task<OrderDto> CreateAsync(CreateOrderDto dto);
    Task<bool> UpdateAsync(Guid id, UpdateOrderDto dto);
}

