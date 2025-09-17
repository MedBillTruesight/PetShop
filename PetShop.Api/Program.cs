using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PetShop.Api.Data;
using PetShop.Api.Repository;
using PetShop.Api.Services;
using PetShop.Api.Validation;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
// AddAsync services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<PetShopDbContext>(opt =>
    opt.UseInMemoryDatabase("PetShopDb"));

// Repositories
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();

// Services
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

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
