using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using PetShop.Api.Swagger;
using PetShop.Application.DTOs;
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
            options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
            options.SwaggerGeneratorOptions.SwaggerDocs.Clear();

            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Pet Shop API",
                Version = "1.0",
                Description = "REST API for Pet Shop management. Manages customers, orders, and pets. " +
                    "Orders follow a lifecycle: Open → Processing → Delivered. Cost calculations (estimated vs actual) depend on order status.",
                Contact = new Microsoft.OpenApi.Models.OpenApiContact
                {
                    Name = "Pet Shop API Support",
                    Email = "api-support@petshop.example.com",
                    Url = new Uri("https://github.com/petshop/api", UriKind.Absolute)
                }
            });

            // Include XML comments from API and Application assemblies
            var apiXml = Path.Combine(AppContext.BaseDirectory, $"{typeof(Program).Assembly.GetName().Name}.xml");
            if (File.Exists(apiXml))
                options.IncludeXmlComments(apiXml);
            var appXml = Path.Combine(AppContext.BaseDirectory, $"{typeof(CreateCustomerRequest).Assembly.GetName().Name}.xml");
            if (File.Exists(appXml))
                options.IncludeXmlComments(appXml);

            options.OperationFilter<SwaggerExampleOperationFilter>();
            options.DocumentFilter<SwaggerTagOrderDocumentFilter>();
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
        // Enable Swagger in Development or when explicitly enabled
        if (app.Environment.IsDevelopment() || 
            builder.Configuration.GetValue<bool>("EnableSwagger", false))
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

        // Only use HTTPS redirection if HTTPS is configured
        if (app.Configuration.GetValue<bool>("UseHttpsRedirection", false))
        {
            app.UseHttpsRedirection();
        }

        // Register global exception handler middleware (must be early in pipeline)
        app.UseMiddleware<Middleware.GlobalExceptionHandlerMiddleware>();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
