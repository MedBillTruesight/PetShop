using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PetShop.Application.DTOs;
using PetShop.Application.Features.Customer.Interfaces;
using PetShop.Application.Features.Customer.Services;
using PetShop.Application.Features.Order.Interfaces;
using PetShop.Application.Features.Order.Services;
using PetShop.Application.Features.Pet.Interfaces;
using PetShop.Application.Features.Pet.Services;
using PetShop.Application.Mappers;
using PetShop.Application.MappingProfiles;
using PetShop.Domain.Enums;
using PetShop.Domain.Interfaces;
using PetShop.Infrastructure.Persistence;
using PetShop.Infrastructure.Repositories;

namespace PetShop.Test;

public abstract class IntegrationTestBase : IDisposable
{
    protected readonly PetShopDbContext _context;
    protected readonly ICustomerService _customerService;
    protected readonly IPetService _petService;
    protected readonly IOrderService _orderService;

    protected IntegrationTestBase()
    {
        var services = new ServiceCollection();

        // Add in-memory database
        services.AddDbContext<PetShopDbContext>(options =>
            options.UseInMemoryDatabase($"PetShopTest_{Guid.NewGuid()}"));

        // Add repositories
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IPetRepository, PetRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        // Add AutoMapper
        services.AddAutoMapper(typeof(CustomerMappingProfile));
        services.AddAutoMapper(typeof(OrderMappingProfile));
        services.AddAutoMapper(typeof(PetMappingProfile));

        // Add mappers
        services.AddScoped<ICustomerMapper, CustomerMapper>();
        services.AddScoped<IPetMapper, PetMapper>();
        services.AddScoped<IOrderMapper, OrderMapper>();

        // Add services
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IPetService, PetService>();
        services.AddScoped<IOrderService, OrderService>();

        var serviceProvider = services.BuildServiceProvider();
        _context = serviceProvider.GetRequiredService<PetShopDbContext>();
        _customerService = serviceProvider.GetRequiredService<ICustomerService>();
        _petService = serviceProvider.GetRequiredService<IPetService>();
        _orderService = serviceProvider.GetRequiredService<IOrderService>();

        // Ensure database is created
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context?.Dispose();
    }

    protected async Task<CustomerDto> CreateTestCustomerAsync()
    {
        var createCustomerDto = new CreateCustomerDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Phone = "123-456-7890",
            Address = "123 Main St"
        };

        return await _customerService.CreateCustomerAsync(createCustomerDto);
    }

    protected async Task<PetDto> CreateTestPetAsync()
    {
        var createPetDto = new CreatePetDto
        {
            Name = "Buddy",
            Price = 500.00m,
            Kind = PetKind.Dog,
            Color = "Golden",
            Breed = "Golden Retriever",
            AgeInMonths = 24,
            Description = "Friendly and energetic dog"
        };

        return await _petService.CreatePetAsync(createPetDto);
    }

    protected async Task<OrderDto> CreateTestOrderAsync(int customerId)
    {
        var createOrderDto = new CreateOrderDto
        {
            CustomerId = customerId,
            PickupDate = DateTime.UtcNow.AddDays(1)
        };

        return await _orderService.CreateOrderAsync(createOrderDto);
    }
} 