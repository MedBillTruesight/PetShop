using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PetShop.Application.DTOs;
using PetShop.Application.Features.Customers;
using PetShop.Application.Features.Orders;
using PetShop.Application.Features.Pets;
using PetShop.Application.Interfaces.Mappers;
using PetShop.Application.Interfaces.Services;
using PetShop.Application.Mappers;
using PetShop.Application.Profiles;
using PetShop.Domain.Enums;
using PetShop.Domain.Interfaces.Repositories;
using PetShop.Infrastructure.Persistence;
using PetShop.Infrastructure.Repositories;

namespace PetShop.Tests;

public class IntegrationTestBase : IDisposable
{
    protected readonly PetShopDbContext _context;
    protected readonly ICustomerService _customerService;
    protected readonly IPetService _petService;
    protected readonly IOrderService _orderService;
    
    public IntegrationTestBase()
    {
        //add common setup for integration tests here
        var services = new ServiceCollection();
    
        //add db in memory or test db setup here
        services.AddDbContext<PetShopDbContext>(options =>
            options.UseInMemoryDatabase($"PetShopTestDb-{Guid.NewGuid()}"));
        
        //add the repositories and services needed for tests
        services.AddScoped<IPetRepository, PetRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        
        //add the services needed for tests
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IPetService, PetService>();
        services.AddScoped<IOrderService, OrderService>();
        
        //mapping profiles, mediators
        services.AddAutoMapper(typeof(CustomerMappingProfile));
        services.AddAutoMapper(typeof(PetMappingProfile));
        services.AddAutoMapper(typeof(OrderMappingProfile));
        
        //
        services.AddScoped<ICustomerMapper, CustomerMapper>();
        services.AddScoped<IPetMapper, PetMapper>();
        services.AddScoped<IOrderMapper, OrderMapper>();
        
        var serviceProvider = services.BuildServiceProvider();
        _context = serviceProvider.GetRequiredService<PetShopDbContext>();
        _customerService = serviceProvider.GetRequiredService<ICustomerService>();
        _petService = serviceProvider.GetRequiredService<IPetService>();
        _orderService = serviceProvider.GetRequiredService<IOrderService>();
        
        //ensure db is created
        _context.Database.EnsureCreated();
        

    }

    public void Dispose()
    {
        //dispose resources if needed
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
    
    protected async Task<CustomerDto> CreateCustomerTestAsync()
    {
        var createCustomerDto = new CreateCustomerDto()
        {
            FirstName = "Samuel",
            LastName = "Kirigha",
            Email = "sam.kkk@example.com",
            Phone = "231-342-342-3423",
            Address = "123 Nairobi St"
        };

        return await _customerService.CreateCustomerAsync(createCustomerDto);
    }
    
    
    protected async Task<PetDto> CreatePetTestAsync()
    {
        var createPetDto = new CreatePetDto
        {
            Name = "Buddy",
            Price = 150.00m,
            Kind = PetKind.Dog,
            Color = "Golden",
            Breed = "Golden Retriever",
            IsVaccinated =  false,
            AgeInMonths = 24,
            Description = "Friendly and energetic dog"
        };

        return await _petService.CreatePetAsync(createPetDto);
    }
    
    protected async Task<OrderDto> CreateOrderTestAsync(Guid customerId)
    {
        var createOrderDto = new CreateOrderDto
        {
            CustomerId = customerId,
            PickupDate = DateTime.UtcNow.AddDays(1)
        };

        return await _orderService.CreateOrderAsync(createOrderDto);
    }
}