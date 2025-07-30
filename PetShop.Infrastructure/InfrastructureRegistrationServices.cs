
using Microsoft.Extensions.DependencyInjection;
using PetShop.Domain.Interfaces;
using PetShop.Infrastructure.Repositories;

namespace PetShop.Infrastructure;

public static class InfrastructureRegistrationServices
{
	public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
	{
		// Register your services here
		services.AddScoped<ICustomerRepository, CustomerRepository>();
		services.AddScoped<IOrderRepository, OrderRepository>();
		services.AddScoped<IPetRepository, PetRepository>();

		return services;
	}
}