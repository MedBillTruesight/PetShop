using Microsoft.EntityFrameworkCore;
using PetShop.Application;
using PetShop.Infrastructure;
using PetShop.Infrastructure.Persistence;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<PetShopDbContext>(options =>
    options.UseInMemoryDatabase("PetShopDb"));

// Application services
builder.Services.AddApplicationServices();

// Infrastructure services (e.g., repositories)
builder.Services.AddInfrastructureServices();

// Register FluentValidation behavior (auto-validates on endpoints)
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

// Redirect root to Swagger UI
app.MapGet("/", () => Results.Redirect("/swagger/index.html"));

app.MapControllers();

app.Run();