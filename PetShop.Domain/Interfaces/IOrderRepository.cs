using PetShop.Domain.Entities;

namespace PetShop.Domain.Interfaces;

public interface IOrderRepository
{
    Task<Order> CreateOrderAsync(Order order);
    Task<Order?> GetOrderByIdAsync(int id);
    Task<OrderPet> AddOrderPetAsync(OrderPet orderPet);
    Task<OrderPet> RemoveOrderPetAsync(OrderPet orderPet);
    Task<List<Order>> GetAllOrdersAsync();
    Task<List<Order>> GetOrdersByCustomerAsync(int customerId);
    Task<Order?> UpdateOrderAsync(Order order);


    // Task<Order?> DeleteOrderByIdAsync(int id);
}