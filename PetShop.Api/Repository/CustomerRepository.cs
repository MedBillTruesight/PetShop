using Microsoft.EntityFrameworkCore;
using PetShop.Api.Data;
using PetShop.Api.Models;
using System;

namespace PetShop.Api.Repository
{
    public class CustomerRepository: ICustomerRepository
    {
        private readonly PetShopDbContext _context;

        public CustomerRepository(PetShopDbContext dbContext)
        {
            _context = dbContext;
        }

        public async Task<Customer> AddAsync(Customer customer)
        {
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task<bool> Delete(Guid id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return false;

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Customer>> GetAll()
        {
            return await _context.Customers
            .Include(c => c.Orders)
            .ThenInclude(o => o.Pets)
            .ToListAsync();
        }

        public async Task<Customer> GetById(Guid id)
        {
            return await _context.Customers
            .Include(c => c.Orders)
            .ThenInclude(o => o.Pets)
            .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Customer> Update(Customer customer)
        {
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Customers.AnyAsync(c => c.Id == id);
        }
    }
}
