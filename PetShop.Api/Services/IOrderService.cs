using PetShop.Api.Dtos;

namespace PetShop.Api.Services
{
    public interface IOrderService
    {
        Task<OrderDto> CreateOrderAsync(CreateOrderDto dto);
        Task<OrderDto?> GetOrderAsync(Guid orderId);
        Task<OrderDto> UpdateOrderAsync(Guid orderId, UpdateOrderDto dto);
        Task<OrderDto> MarkAsDeliveredAsync(Guid orderId);
    }
}
