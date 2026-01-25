using PetShop.Domain.Entities;

namespace PetShop.Domain.Interfaces.Repositories;
public interface IOrderRepository
{
    Task<Order> CreateOrderAsync(Order order);
    Task<Order?> GetOrderByIdAsync(Guid id);
    Task<OrderPet> AddOrderPetAsync(OrderPet orderPet);
    Task<OrderPet> RemoveOrderPetAsync(OrderPet orderPet);
    Task<List<Order>> GetAllOrdersAsync();
    Task<List<Order>> GetOrdersByCustomerAsync(Guid customerId);
    Task<Order?> UpdateOrderAsync(Order order);
}