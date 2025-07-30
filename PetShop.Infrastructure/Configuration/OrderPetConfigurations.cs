using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetShop.Domain.Entities;

namespace PetShop.Infrastructure.Configuration;

public class OrderPetConfigurations:IEntityTypeConfiguration<OrderPet>
{
    public void Configure(EntityTypeBuilder<OrderPet> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.PetId).IsRequired();
        builder.Property(o => o.OrderId).IsRequired();

        builder.HasOne(e => e.Order)
            .WithMany(c => c.OrderPets)
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Pet)
            .WithMany(p => p.OrderPets)
            .HasForeignKey(e => e.PetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
