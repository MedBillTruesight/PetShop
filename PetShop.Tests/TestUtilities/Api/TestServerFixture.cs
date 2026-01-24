using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PetShop.Api;
using PetShop.Infrastructure;

/// <summary>
/// Test fixture providing a configured test server for API integration tests.
/// Sets up in-memory database and test services.
/// </summary>
public class TestServerFixture : WebApplicationFactory<Program>, IDisposable
{
    private readonly string _databaseName;

    public TestServerFixture()
    {
        _databaseName = Guid.NewGuid().ToString();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<PetShopDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add in-memory database for testing
            services.AddDbContext<PetShopDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });

            // Ensure the database is created
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<PetShopDbContext>();
            db.Database.EnsureCreated();
        });
    }

    /// <summary>
    /// Gets a configured HttpClient for testing.
    /// </summary>
    public HttpClient CreateClientWithJsonDefaults()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        return client;
    }

    /// <summary>
    /// Gets the service provider for direct service access in tests.
    /// </summary>
    public IServiceProvider Services => Server.Services;

    /// <summary>
    /// Executes an action within a service scope.
    /// </summary>
    public async Task ExecuteInScopeAsync(Func<IServiceProvider, Task> action)
    {
        using var scope = Services.CreateScope();
        await action(scope.ServiceProvider);
    }

    /// <summary>
    /// Executes a function within a service scope and returns a result.
    /// </summary>
    public async Task<T> ExecuteInScopeAsync<T>(Func<IServiceProvider, Task<T>> func)
    {
        using var scope = Services.CreateScope();
        return await func(scope.ServiceProvider);
    }

    public new void Dispose()
    {
        base.Dispose();
    }
}
