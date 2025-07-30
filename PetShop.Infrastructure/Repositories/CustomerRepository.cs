using Microsoft.EntityFrameworkCore;
using PetShop.Domain.Entities;
using PetShop.Domain.Interfaces;
using PetShop.Infrastructure.Persistence;

namespace PetShop.Infrastructure.Repositories;

public class CustomerRepository: ICustomerRepository
{
    private readonly PetShopDbContext _context;

    public CustomerRepository(PetShopDbContext context)
    {
        _context = context;
    }
    
    public async Task<Customer> CreateCustomerAsync(Customer customer)
    {
        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();
        return customer;
    }

    public async Task<Customer?> UpdateCustomerAsync(Customer customer)
    {
        var existingCustomer= await _context.Customers.FirstOrDefaultAsync(c => c.Id == customer.Id);
        
        if (existingCustomer != null)
        {
            existingCustomer.FirstName = customer.FirstName;
            existingCustomer.LastName = customer.LastName;
            existingCustomer.Email = customer.Email;
            existingCustomer.Phone = customer.Phone;
            existingCustomer.Address = customer.Address;

        }
        await _context.SaveChangesAsync();
        return existingCustomer;
    }

    public async Task<Customer?> GetCustomerByIdAsync(int id)
    {
        return  await _context.Customers
            .Include(c => c.Orders)
            .FirstOrDefaultAsync(c=>c.Id == id);
    }

    public async Task<Customer?> DeleteCustomerByIdAsync(int id)
    {
        var customerToDelete = await _context.Customers.FirstOrDefaultAsync(c => c.Id == id);
        
        if (customerToDelete == null)
        {
            return null;

        }
        
        _context.Customers.Remove(customerToDelete);
        await _context.SaveChangesAsync();
        return customerToDelete;
    }

    public async Task<List<Customer>> GetAllCustomersAsync()
    {
        return await _context.Customers.AsNoTracking().ToListAsync();
    }
}