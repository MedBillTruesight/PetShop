using Microsoft.EntityFrameworkCore;
using PetShop.Domain;

namespace PetShop.Infrastructure;

/// <summary>
/// Entity Framework Core database context for the Pet Shop application.
/// Provides access to Customer, Order, and Pet entities using InMemory database provider.
/// </summary>
public class PetShopDbContext : DbContext
{
    /// <summary>
    /// Gets or sets the customers in the database.
    /// </summary>
    public DbSet<Customer> Customers { get; set; } = null!;

    /// <summary>
    /// Gets or sets the orders in the database.
    /// </summary>
    public DbSet<Order> Orders { get; set; } = null!;

    /// <summary>
    /// Gets or sets the pets in the database.
    /// </summary>
    public DbSet<Pet> Pets { get; set; } = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="PetShopDbContext"/> class.
    /// </summary>
    /// <param name="options">The options for this context.</param>
    public PetShopDbContext(DbContextOptions<PetShopDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Configures the model that was discovered by convention from the entity types
    /// exposed in <see cref="DbSet{TEntity}"/> properties on this context.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureCustomerEntity(modelBuilder);
        ConfigureOrderEntity(modelBuilder);
        ConfigurePetEntity(modelBuilder);
    }

    private static void ConfigureCustomerEntity(ModelBuilder modelBuilder)
    {
        var customer = modelBuilder.Entity<Customer>();

        // Primary key
        customer.HasKey(c => c.Id);

        // Properties
        customer.Property(c => c.Id)
            .IsRequired();

        customer.Property(c => c.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        customer.Property(c => c.LastName)
            .IsRequired()
            .HasMaxLength(100);

        customer.Property(c => c.Email)
            .HasMaxLength(255);

        customer.Property(c => c.Phone)
            .HasMaxLength(50);

        // Navigation property - one-to-many relationship with Orders
        customer.HasMany(c => c.Orders)
            .WithOne(o => o.Customer)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureOrderEntity(ModelBuilder modelBuilder)
    {
        var order = modelBuilder.Entity<Order>();

        // Primary key
        order.HasKey(o => o.Id);

        // Properties
        order.Property(o => o.Id)
            .IsRequired();

        order.Property(o => o.CustomerId)
            .IsRequired();

        order.Property(o => o.Status)
            .IsRequired()
            .HasConversion<int>();

        order.Property(o => o.PickupDate)
            .IsRequired()
            .HasConversion<DateTime>(
                v => v.ToDateTime(TimeOnly.MinValue),
                v => DateOnly.FromDateTime(v));

        order.Property(o => o.ActualCost)
            .HasPrecision(18, 2);

        // Foreign key relationship with Customer
        order.HasOne(o => o.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Navigation property - one-to-many relationship with Pets
        order.HasMany(o => o.Pets)
            .WithOne(p => p.Order)
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigurePetEntity(ModelBuilder modelBuilder)
    {
        var pet = modelBuilder.Entity<Pet>();

        // Primary key
        pet.HasKey(p => p.Id);

        // Properties
        pet.Property(p => p.Id)
            .IsRequired();

        pet.Property(p => p.OrderId)
            .IsRequired();

        pet.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        pet.Property(p => p.Price)
            .IsRequired()
            .HasPrecision(18, 2);

        pet.Property(p => p.Kind)
            .HasMaxLength(50);

        pet.Property(p => p.Color)
            .HasMaxLength(50);

        // Foreign key relationship with Order
        pet.HasOne(p => p.Order)
            .WithMany(o => o.Pets)
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
