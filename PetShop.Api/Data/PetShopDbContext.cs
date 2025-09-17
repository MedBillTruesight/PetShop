using Microsoft.EntityFrameworkCore;
using PetShop.Api.Models;

namespace PetShop.Api.Data
{
    public class PetShopDbContext : DbContext
    {
        public PetShopDbContext(DbContextOptions<PetShopDbContext> options) : base(options)
        {
            
        }
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<Pet> Pets => Set<Pet>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(c => c.LastName).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Email).HasMaxLength(100);
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.Property(o => o.Status).IsRequired();
                entity.Property(o => o.PickupDate).IsRequired();

                entity.HasOne(o => o.Customer)
                      .WithMany(c => c.Orders)
                      .HasForeignKey(o => o.CustomerId);
            });

            modelBuilder.Entity<Pet>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(50);
                entity.Property(p => p.Kind).IsRequired().HasMaxLength(50);
                entity.Property(p => p.Color).HasMaxLength(50);
                entity.Property(p => p.Breed).HasMaxLength(50);
                entity.Property(p => p.Price).HasPrecision(18, 2);

                entity.HasOne(p => p.Order)
                      .WithMany(o => o.Pets)
                      .HasForeignKey(p => p.OrderId);
            });
        }
    }
}
