using Microsoft.EntityFrameworkCore;
using PetShop.API.Data;

namespace PetShop.Tests;

public static class TestDbContextFactory
{
    public static PetShopContext Create(string dbName)
    {
        var options = new DbContextOptionsBuilder<PetShopContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new PetShopContext(options);
    }
}