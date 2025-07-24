using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PetShop.API.Controllers;
using PetShop.API.DTOs;
using PetShop.API.Services;
using Xunit;

namespace PetShop.Tests.Controllers;

public class CustomersControllerTests
{
    private readonly IMapper _mapper;

    public CustomersControllerTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<CustomerProfile>());
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task Create_Should_Return_CreatedAtAction()
    {
        var context = TestDbContextFactory.Create("Controller_Create");
        var service = new CustomerService(context, _mapper);
        var controller = new CustomersController(service);

        var dto = new CreateCustomerDto
        {
            FirstName = "Jermaine",
            LastName = "Wando",
            Email = "jermaine@example.com"
        };

        var result = await controller.Create(dto);
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnedDto = Assert.IsType<CustomerDto>(createdResult.Value);

        Assert.Equal("Jermaine Wando", returnedDto.FullName);
    }

    [Fact]
    public async Task GetById_Should_Return_NotFound_If_NotExists()
    {
        var context = TestDbContextFactory.Create("Controller_GetById");
        var service = new CustomerService(context, _mapper);
        var controller = new CustomersController(service);

        var result = await controller.GetById(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Update_Should_Return_Ok()
    {
        var context = TestDbContextFactory.Create("Controller_Update");
        var service = new CustomerService(context, _mapper);

        var customer = new PetShop.API.Models.Customer
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane@update.com"
        };
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var controller = new CustomersController(service);

        var dto = new UpdateCustomerDto { FirstName = "Janet" };

        var result = await controller.Update(customer.Id, dto);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var updatedDto = Assert.IsType<CustomerDto>(okResult.Value);

        Assert.Equal("Janet Smith", updatedDto.FullName);
    }
}
