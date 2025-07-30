using Microsoft.EntityFrameworkCore;
using PetShop.Domain.Entities;
using PetShop.Domain.Interfaces;
using PetShop.Infrastructure.Persistence;

namespace PetShop.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly PetShopDbContext _context;

    public OrderRepository(PetShopDbContext context)
    {
        _context = context;
    }

    public async Task<OrderPet> AddOrderPetAsync(OrderPet orderPet)
    {
        await _context.OrderPets.AddAsync(orderPet);
        await _context.SaveChangesAsync();
        return orderPet;
    }

    public async Task<Order> CreateOrderAsync(Order order)
    {
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();
        return order;
    }

	public async Task<List<Order>> GetAllOrdersAsync()
	{
		return await _context.Orders
            .Include(o => o.OrderPets)
            .ThenInclude(op => op.Pet)
        .AsNoTracking().ToListAsync();
	}

	public async Task<Order?> GetOrderByIdAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.OrderPets)
            .ThenInclude(op => op.Pet)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

	public async Task<List<Order>> GetOrdersByCustomerAsync(int customerId)
	{
        return await _context.Orders
            .Include(o => o.OrderPets)
            .ThenInclude(op => op.Pet)
            .Where(o => o.CustomerId == customerId)
            .AsNoTracking()
            .ToListAsync();
    }

	public async Task<OrderPet> RemoveOrderPetAsync(OrderPet orderPet)
	{
		_context.OrderPets.Remove(orderPet);
		await _context.SaveChangesAsync();
		return orderPet;
	}

	public async Task<Order?> UpdateOrderAsync(Order order)
	{
		_context.Orders.Update(order);
		await _context.SaveChangesAsync();
		return order;
	}
}