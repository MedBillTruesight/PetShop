using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PetShop.API.Data;
using PetShop.API.DTOs;
using PetShop.API.Models;
using PetShop.API.Services.Interfaces;

namespace PetShop.API.Services;

public class OrderService :IOrderService
{
    private readonly PetShopContext _context;
    private readonly IMapper _mapper;

    public OrderService(PetShopContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<OrderDto?> GetByIdAsync(Guid id)
    {
        var order = await _context.Orders
            .Include(o => o.Pets)
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.Id == id);

        return order == null ? null : MapOrderToDto(order);
    }

    public async Task<List<OrderDto>> GetAllAsync()
    {
        var orders = await _context.Orders
            .Include(o => o.Pets)
            .Include(o => o.Customer)
            .ToListAsync();

        return orders.Select(MapOrderToDto).ToList();
    }

    public async Task<OrderDto> CreateAsync(CreateOrderDto dto)
    {
        if (dto.PickupDate < DateTime.Today)
            throw new ArgumentException("Pickup date cannot be in the past.");

        var customer = await _context.Customers.FindAsync(dto.CustomerId);
        if (customer == null)
            throw new ArgumentException("Invalid customer ID.");

        var hasOpenOrder = await _context.Orders.AnyAsync(o =>
            o.CustomerId == dto.CustomerId &&
            (o.Status == OrderStatus.Open || o.Status == OrderStatus.Processing));

        if (hasOpenOrder)
            throw new InvalidOperationException("Customer already has an active order.");

        var pets = await _context.Pets.Where(p => dto.PetIds.Contains(p.Id)).ToListAsync();

        if (pets.Count != dto.PetIds.Count)
            throw new ArgumentException("One or more Pet IDs are invalid.");

        var order = new Order
        {
            CustomerId = dto.CustomerId,
            PickupDate = dto.PickupDate,
            Pets = pets
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(order.Id) ?? throw new Exception("Failed to retrieve created order.");
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateOrderDto dto)
    {
        var order = await _context.Orders.Include(o => o.Pets).FirstOrDefaultAsync(o => o.Id == id);
        if (order == null) return false;

        if (dto.PickupDate.HasValue)
        {
            if (dto.PickupDate.Value < DateTime.Today)
                throw new ArgumentException("Pickup date cannot be in the past.");

            order.PickupDate = dto.PickupDate.Value;
        }

        if (!string.IsNullOrWhiteSpace(dto.Status) &&
            Enum.TryParse(dto.Status, true, out OrderStatus newStatus))
        {
            if (IsValidStatusTransition(order.Status, newStatus, order.Pets))
            {
                order.Status = newStatus;

                if (newStatus == OrderStatus.Delivered)
                    order.ActualTotalCost = order.Pets.Sum(p => p.Price);
            }
        }

        if (dto.PetIds != null)
            throw new InvalidOperationException("Modifying pets is not allowed on update.");

        await _context.SaveChangesAsync();
        return true;
    }

    private static bool IsValidStatusTransition(OrderStatus current, OrderStatus next, ICollection<Pet> pets)
    {
        return (current == OrderStatus.Open && next == OrderStatus.Processing && pets.Any()) ||
               (current == OrderStatus.Processing && next == OrderStatus.Delivered);
    }

    private OrderDto MapOrderToDto(Order order)
    {
        var dto = _mapper.Map<OrderDto>(order);
        dto.EstimatedCost = order.Status == OrderStatus.Delivered ? null : order.Pets.Sum(p => p.Price);
        dto.ActualCost = order.Status == OrderStatus.Delivered ? order.ActualTotalCost : null;
        dto.Customer.EstimatedPaymentDue = 0;
        dto.Customer.ActualPaymentDue = 0;
        return dto;
    }
}
