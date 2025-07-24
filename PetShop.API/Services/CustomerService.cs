using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PetShop.API.Data;
using PetShop.API.DTOs;
using PetShop.API.Models;
using PetShop.API.Services.Interfaces;

public class CustomerService : ICustomerService
{
    private readonly PetShopContext _context;
    private readonly IMapper _mapper;

    public CustomerService(PetShopContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<CustomerDto>> GetAllAsync()
    {
        var customers = await _context.Customers
            .Include(c => c.Orders)
            .ThenInclude(o => o.Pets)
            .ToListAsync();

        return customers.Select(c =>
        {
            var dto = _mapper.Map<CustomerDto>(c);
            dto.EstimatedPaymentDue = GetEstimatedDue(c);
            dto.ActualPaymentDue = GetActualDue(c);
            return dto;
        }).ToList();
    }

    public async Task<CustomerDto?> GetByIdAsync(Guid id)
    {
        var customer = await _context.Customers
            .Include(c => c.Orders)
            .ThenInclude(o => o.Pets)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer == null) return null;

        var dto = _mapper.Map<CustomerDto>(customer);
        dto.EstimatedPaymentDue = GetEstimatedDue(customer);
        dto.ActualPaymentDue = GetActualDue(customer);
        return dto;
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            var exists = await _context.Customers.AnyAsync(c => c.Email == dto.Email);
            if (exists) throw new InvalidOperationException("A customer with this email already exists.");
        }

        var customer = new Customer
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var saved = await _context.Customers
            .Include(c => c.Orders)
            .ThenInclude(o => o.Pets)
            .FirstAsync(c => c.Id == customer.Id);

        var result = _mapper.Map<CustomerDto>(saved);
        result.EstimatedPaymentDue = GetEstimatedDue(saved);
        result.ActualPaymentDue = GetActualDue(saved);
        return result;
    }

    public async Task<CustomerDto?> UpdateAsync(Guid id, UpdateCustomerDto dto)
    {
        var customer = await _context.Customers
            .Include(c => c.Orders)
            .ThenInclude(o => o.Pets)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer == null) return null;

        if (dto.FirstName != null) customer.FirstName = dto.FirstName;
        if (dto.LastName != null) customer.LastName = dto.LastName;
        if (dto.Email != null) customer.Email = dto.Email;
        if (dto.PhoneNumber != null) customer.PhoneNumber = dto.PhoneNumber;

        await _context.SaveChangesAsync();

        var result = _mapper.Map<CustomerDto>(customer);
        result.EstimatedPaymentDue = GetEstimatedDue(customer);
        result.ActualPaymentDue = GetActualDue(customer);
        return result;
    }

    private decimal GetEstimatedDue(Customer customer)
    {
        return customer.Orders
            .Where(o => o.Status != OrderStatus.Delivered)
            .SelectMany(o => o.Pets)
            .Sum(p => p.Price);
    }

    private decimal GetActualDue(Customer customer)
    {
        return customer.Orders
            .Where(o => o.Status == OrderStatus.Delivered)
            .Sum(o => o.ActualTotalCost ?? 0);
    }
}
