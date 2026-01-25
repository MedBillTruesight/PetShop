using Microsoft.Extensions.DependencyInjection;
using PetShop.Application.Features.Customers;
using PetShop.Application.Features.Orders;
using PetShop.Application.Features.Pets;
using PetShop.Application.Interfaces.Mappers;
using PetShop.Application.Interfaces.Services;
using PetShop.Application.Mappers;
using PetShop.Application.Profiles;

namespace PetShop.Application;
public static class ApplicationMainServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddTransient<ICustomerMapper, CustomerMapper>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddTransient<IOrderMapper, OrderMapper>();
        services.AddScoped<IPetService, PetService>();
        services.AddTransient<IPetMapper, PetMapper>();

        services.AddAutoMapper(cfg => cfg.AddMaps(typeof(CustomerMappingProfile).Assembly));
        services.AddAutoMapper(cfg => cfg.AddMaps(typeof(OrderMappingProfile).Assembly));
        services.AddAutoMapper(cfg => cfg.AddMaps(typeof(PetMappingProfile).Assembly));

        return services;
    }
    
}