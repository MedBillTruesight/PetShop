using Microsoft.EntityFrameworkCore;
using PetShop.Application;
using PetShop.Application.MappingProfiles;
using PetShop.Infrastructure;
using PetShop.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework with In-Memory Database
builder.Services.AddDbContext<PetShopDbContext>(options =>
    options.UseInMemoryDatabase("PetShopDb"));

// Application layer services
builder.Services.AddApplicationServices();

// Infrastructure layer services (e.g., repositories)
builder.Services.AddInfrastructureServices();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();