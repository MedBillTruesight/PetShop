using PetShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace PetShop.Infrastructure.Persistence;

public class PetShopDbContext : DbContext
{
	public PetShopDbContext(DbContextOptions<PetShopDbContext> options) : base(options)
	{
	}

	public DbSet<Pet> Pets { get; set; } = null!;
	public DbSet<Customer> Customers { get; set; } = null!;
	public DbSet<Order> Orders { get; set; } = null!;
	public DbSet<OrderPet> OrderPets { get; set; } = null!;

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.ApplyConfigurationsFromAssembly(typeof(PetShopDbContext).Assembly);
	}
}