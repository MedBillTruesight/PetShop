using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using PetShop.Application.Repositories;
using PetShop.Application.Services;
using PetShop.Infrastructure;
using PetShop.Infrastructure.Repositories;

namespace PetShop.Api;

/// <summary>
/// Main entry point for the Pet Shop API application.
/// </summary>
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure API versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader()
    );
});

// Add API versioning explorer for Swagger
builder.Services.AddVersionedApiExplorer(setup =>
{
    setup.GroupNameFormat = "'v'VVV";
    setup.SubstituteApiVersionInUrl = true;
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Configure Swagger to support API versioning
    options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
    
    // Configure Swagger documents for each API version using a factory
    options.SwaggerGeneratorOptions.SwaggerDocs.Clear();
    
    // Add Swagger document for v1
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Pet Shop API",
        Version = "1.0",
        Description = "REST API for Pet Shop management"
    });
});

// Configure DbContext with InMemory provider
builder.Services.AddDbContext<PetShopDbContext>(options =>
    options.UseInMemoryDatabase("PetShopDb"));

// Register repositories (Scoped lifetime - one per HTTP request)
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Register application services (Scoped lifetime - one per HTTP request)
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<OrderService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var apiVersionDescriptionProvider = app.Services
            .GetRequiredService<Microsoft.AspNetCore.Mvc.ApiExplorer.IApiVersionDescriptionProvider>();

        foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                $"Pet Shop API {description.GroupName.ToUpperInvariant()}");
        }
    });
}

app.UseHttpsRedirection();

// Register global exception handler middleware (must be early in pipeline)
app.UseMiddleware<Middleware.GlobalExceptionHandlerMiddleware>();

app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
