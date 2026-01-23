using Microsoft.EntityFrameworkCore;
using PetShop.Application.Repositories;
using PetShop.Application.Services;
using PetShop.Infrastructure;
using PetShop.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
