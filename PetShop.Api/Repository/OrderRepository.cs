using Microsoft.EntityFrameworkCore;
using PetShop.Api.Data;
using PetShop.Api.Models;

namespace PetShop.Api.Repository
{
    public class OrderRepository: IOrderRepository
    {
        private readonly PetShopDbContext _context;
        public OrderRepository(PetShopDbContext context)
        {
            _context = context;
        }
        public async Task<Order> AddAsync(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }
        public async Task<Pet> AddPetAsync(Pet pet)
        {
            _context.Pets.Add(pet);
            await _context.SaveChangesAsync();
            return pet;
        }
        public async Task<bool> Delete(Guid id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return false;
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<Order> GetById(Guid id)
        {
            return await _context.Orders
                .Include(o => o.Pets)
                .FirstOrDefaultAsync(o => o.Id == id);
        }
        public async Task<IEnumerable<Order>> GetByCustomerId(Guid customerId)
        {
            return await _context.Orders
                .Where(o => o.CustomerId == customerId)
                .Include(o => o.Pets)
                .ToListAsync();
        }
        public async Task<Pet> GetPetById(Guid id)
        {
            return await _context.Pets.FindAsync(id);
        }
        public async Task<bool> RemovePet(Guid id)
        {
            var pet = await _context.Pets.FindAsync(id);
            if (pet == null) return false;
            _context.Pets.Remove(pet);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<Order> Update(Order order)
        {
            var existingOrder = await _context.Orders.FindAsync(order.Id);
            if (existingOrder == null) return null;
            existingOrder.Status = order.Status;
            existingOrder.ActualCost = order.ActualCost;
            _context.Orders.Update(existingOrder);
            await _context.SaveChangesAsync();
            return existingOrder;
        }
    }
}
