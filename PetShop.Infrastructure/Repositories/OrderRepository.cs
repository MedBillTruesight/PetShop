using Microsoft.EntityFrameworkCore;
using PetShop.Application.Repositories;
using PetShop.Domain;

namespace PetShop.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Order entity operations using Entity Framework Core.
/// </summary>
public class OrderRepository : IOrderRepository
{
    private readonly PetShopDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public OrderRepository(PetShopDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Gets an order by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the order.</param>
    /// <returns>The order if found, otherwise null.</returns>
    public async Task<Order?> GetByIdAsync(Guid id)
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Pets)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    /// <summary>
    /// Gets all orders.
    /// </summary>
    /// <returns>A collection of all orders.</returns>
    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Pets)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all orders for a specific customer.
    /// </summary>
    /// <param name="customerId">The unique identifier of the customer.</param>
    /// <returns>A collection of orders for the specified customer.</returns>
    public async Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId)
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Pets)
            .Where(o => o.CustomerId == customerId)
            .ToListAsync();
    }

    /// <summary>
    /// Creates a new order.
    /// </summary>
    /// <param name="order">The order to create.</param>
    /// <returns>The created order.</returns>
    public async Task<Order> CreateAsync(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    /// <summary>
    /// Updates an existing order.
    /// </summary>
    /// <param name="order">The order to update.</param>
    /// <returns>The updated order.</returns>
    public async Task<Order> UpdateAsync(Order order)
    {
        var entry = _context.Entry(order);

        if (entry.State == EntityState.Detached)
        {
            // Entity is not tracked - attach and mark as modified
            // This will also handle new pets in the collection
            _context.Orders.Update(order);
        }
        else
        {
            // Entity is already tracked - ensure new pets are explicitly Added.
            // Workaround for EF Core bug: adding a child to a tracked parent's collection
            // can mark the child as Modified instead of Added, causing UPDATE instead of
            // INSERT and DbUpdateConcurrencyException. We never modify existing pets
            // (only add/remove), so any Modified pet here is a new one wrongly marked.
            foreach (var pet in order.Pets)
            {
                var petEntry = _context.Entry(pet);
                if (petEntry.State == EntityState.Detached)
                {
                    _context.Pets.Add(pet);
                }
                else if (petEntry.State == EntityState.Modified)
                {
                    petEntry.State = EntityState.Added;
                }
            }
        }

        await _context.SaveChangesAsync();
        return order;
    }

    /// <summary>
    /// Deletes an order by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the order to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteAsync(Guid id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order != null)
        {
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
        }
    }
}
