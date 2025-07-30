using Microsoft.Extensions.DependencyInjection;
using PetShop.Application.Features.Customer.Interfaces;
using PetShop.Application.Features.Customer.Services;
using PetShop.Application.Features.Order.Interfaces;
using PetShop.Application.Features.Order.Services;
using PetShop.Application.Features.Pet.Interfaces;
using PetShop.Application.Features.Pet.Services;
using PetShop.Application.Mappers;
using PetShop.Application.MappingProfiles;

namespace PetShop.Application;

public static class ApplicationRegistrationServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddTransient<ICustomerMapper, CustomerMapper>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddTransient<IOrderMapper, OrderMapper>();
        services.AddScoped<IPetService, PetService>();
        services.AddTransient<IPetMapper, PetMapper>();

        services.AddAutoMapper(typeof(CustomerMappingProfile));
        services.AddAutoMapper(typeof(OrderMappingProfile));
        services.AddAutoMapper(typeof(PetMappingProfile));

        return services;
    }
    
}