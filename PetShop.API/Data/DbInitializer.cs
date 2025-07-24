using PetShop.API.Models;

namespace PetShop.API.Data;

public static class DbInitializer
{
    public static void Seed(PetShopContext context)
    {
        if (!context.Pets.Any())
        {
            context.Pets.AddRange(
                new Pet { Name = "Buddy", Kind = "Dog", Color = "Brown", Price = 200 },
                new Pet { Name = "Whiskers", Kind = "Cat", Color = "White", Price = 150 }
            );
        }

        context.SaveChanges();
    }
}