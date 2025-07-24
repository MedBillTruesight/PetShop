using AutoMapper;
using PetShop.API.DTOs;
using PetShop.API.Models;
using PetShop.API.Services;
using Xunit;

namespace PetShop.Tests.Services;

public class CustomerServiceTests
{
    private readonly IMapper _mapper;

    public CustomerServiceTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<CustomerProfile>());
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task CreateAsync_Should_Add_Customer()
    {
        var context = TestDbContextFactory.Create("Create_Add_Customer");
        var service = new CustomerService(context, _mapper);

        var dto = new CreateCustomerDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PhoneNumber = "+123456789"
        };

        var result = await service.CreateAsync(dto);

        Assert.NotNull(result);
        Assert.Equal("John Doe", result.FullName);
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_Customers()
    {
        var context = TestDbContextFactory.Create("GetAll_Customers");
        context.Customers.Add(new Customer { FirstName = "Test", LastName = "User" });
        await context.SaveChangesAsync();

        var service = new CustomerService(context, _mapper);
        var result = await service.GetAllAsync();

        Assert.Single(result);
        Assert.Equal("Test User", result[0].FullName);
    }

    [Fact]
    public async Task CreateAsync_Should_Throw_On_Duplicate_Email()
    {
        var context = TestDbContextFactory.Create("Duplicate_Email");
        context.Customers.Add(new Customer { FirstName = "Jane", LastName = "Doe", Email = "jane@example.com" });
        await context.SaveChangesAsync();

        var service = new CustomerService(context, _mapper);

        var dto = new CreateCustomerDto
        {
            FirstName = "Another",
            LastName = "User",
            Email = "jane@example.com"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(dto));
    }
}
