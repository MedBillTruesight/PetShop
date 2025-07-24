using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PetShop.API.Data;
using PetShop.API.DTOs;
using PetShop.API.Models;
using PetShop.API.Services;
using Xunit;

public class OrderServiceTests
{
    private readonly OrderService _service;
    private readonly PetShopContext _context;
    private readonly IMapper _mapper;

    public OrderServiceTests()
    {
        var options = new DbContextOptionsBuilder<PetShopContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new PetShopContext(options);

        var config = new MapperConfiguration(cfg => cfg.AddProfile(new PetShop.API.MappingProfiles.OrderProfile()));
        _mapper = config.CreateMapper();
        _service = new OrderService(_context, _mapper);

        Seed();
    }

    private void Seed()
    {
        var customer = new Customer { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe" };
        var pet1 = new Pet { Id = Guid.NewGuid(), Name = "Fluffy", Kind = "Cat", Price = 100 };
        var pet2 = new Pet { Id = Guid.NewGuid(), Name = "Buddy", Kind = "Dog", Price = 150 };

        _context.Customers.Add(customer);
        _context.Pets.AddRange(pet1, pet2);
        _context.SaveChanges();
    }

    [Fact]
    public async Task CreateOrder_Succeeds()
    {
        var customer = await _context.Customers.FirstAsync();
        var pet = await _context.Pets.FirstAsync();

        var dto = new CreateOrderDto
        {
            CustomerId = customer.Id,
            PickupDate = DateTime.Today.AddDays(1),
            PetIds = new List<Guid> { pet.Id }
        };

        var result = await _service.CreateAsync(dto);

        Assert.NotNull(result);
        Assert.Equal(customer.Id, result.Customer.Id);
        Assert.Single(result.Pets);
    }

    [Fact]
    public async Task CreateOrder_Throws_When_Date_Past()
    {
        var customer = await _context.Customers.FirstAsync();
        var pet = await _context.Pets.FirstAsync();

        var dto = new CreateOrderDto
        {
            CustomerId = customer.Id,
            PickupDate = DateTime.Today.AddDays(-1),
            PetIds = new List<Guid> { pet.Id }
        };

        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto));
    }

    [Fact]
    public async Task UpdateOrder_ValidStatus_Transitions()
    {
        var customer = await _context.Customers.FirstAsync();
        var pet = await _context.Pets.FirstAsync();

        var dto = new CreateOrderDto
        {
            CustomerId = customer.Id,
            PickupDate = DateTime.Today.AddDays(2),
            PetIds = new List<Guid> { pet.Id }
        };

        var created = await _service.CreateAsync(dto);

        var updateDto = new UpdateOrderDto
        {
            Status = "Processing"
        };

        var updated = await _service.UpdateAsync(created.Id, updateDto);
        Assert.True(updated);
    }
}
