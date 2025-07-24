using Microsoft.AspNetCore.Mvc;
using Moq;
using PetShop.API.Controllers;
using PetShop.API.DTOs;
using PetShop.API.Services.Interfaces;
using Xunit;

public class PetsControllerTests
{
    private readonly Mock<IPetService> _mockService;
    private readonly PetsController _controller;

    public PetsControllerTests()
    {
        _mockService = new Mock<IPetService>();
        _controller = new PetsController(_mockService.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsListOfPets()
    {
        var pets = new List<PetDto>
        {
            new PetDto { Id = Guid.NewGuid(), Name = "Rex", Kind = "Dog", Color = "Brown", Price = 120 },
            new PetDto { Id = Guid.NewGuid(), Name = "Whiskers", Kind = "Cat", Color = "White", Price = 80 }
        };

        _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(pets);

        var result = await _controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedPets = Assert.IsType<List<PetDto>>(okResult.Value);
        Assert.Equal(2, returnedPets.Count);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_IfPetNotFound()
    {
        _mockService.Setup(s => s.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((PetDto?)null);

        var result = await _controller.GetById(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetById_ReturnsPet_IfFound()
    {
        var pet = new PetDto { Id = Guid.NewGuid(), Name = "Buddy", Kind = "Dog", Color = "Black", Price = 150 };
        _mockService.Setup(s => s.GetByIdAsync(pet.Id)).ReturnsAsync(pet);

        var result = await _controller.GetById(pet.Id);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedPet = Assert.IsType<PetDto>(okResult.Value);
        Assert.Equal(pet.Id, returnedPet.Id);
    }

    [Fact]
    public async Task Create_ReturnsCreatedPet()
    {
        var createDto = new CreatePetDto { Name = "Leo", Kind = "Dog", Color = "Golden", Price = 200 };
        var createdPet = new PetDto { Id = Guid.NewGuid(), Name = "Leo", Kind = "Dog", Color = "Golden", Price = 200 };

        _mockService.Setup(s => s.CreateAsync(createDto)).ReturnsAsync(createdPet);

        var result = await _controller.Create(createDto);

        var createdAt = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnedPet = Assert.IsType<PetDto>(createdAt.Value);
        Assert.Equal(createdPet.Id, returnedPet.Id);
    }
}
