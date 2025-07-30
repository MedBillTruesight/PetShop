using Moq;
using PetShop.Application.DTOs;
using PetShop.Application.Features.Pet.Services;
using PetShop.Application.Mappers;
using PetShop.Domain.Entities;
using PetShop.Domain.Enums;
using PetShop.Domain.Interfaces;
using Xunit;

namespace PetShop.Test;

public class PetServiceTests
{
    private readonly Mock<IPetRepository> _mockRepository;
    private readonly Mock<IPetMapper> _mockMapper;
    private readonly PetService _petService;

    public PetServiceTests()
    {
        _mockRepository = new Mock<IPetRepository>();
        _mockMapper = new Mock<IPetMapper>();
        _petService = new PetService(_mockRepository.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task CreatePetAsync_ValidData_ReturnsPetDto()
    {
        // Arrange
        var createPetDto = new CreatePetDto
        {
            Name = "Buddy",
            Price = 500.00m,
            Kind = PetKind.Dog,
            Color = "Golden",
            Breed = "Golden Retriever",
            AgeInMonths = 24,
            Description = "Friendly and energetic dog"
        };

        var pet = new Pet
        {
            Id = 1,
            Name = "Buddy",
            Price = 500.00m,
            Kind = PetKind.Dog,
            Color = "Golden",
            Breed = "Golden Retriever",
            AgeInMonths = 24,
            Description = "Friendly and energetic dog"
        };

        var expectedPetDto = new PetDto
        {
            Id = 1,
            Name = "Buddy",
            Price = 500.00m,
            Kind = PetKind.Dog,
            Color = "Golden",
            Breed = "Golden Retriever",
            AgeInMonths = 24,
            Description = "Friendly and energetic dog"
        };

        _mockMapper.Setup(m => m.ToDomain(createPetDto)).Returns(pet);
        _mockMapper.Setup(m => m.ToDto(pet)).Returns(expectedPetDto);
        _mockRepository.Setup(r => r.CreatePetAsync(pet)).ReturnsAsync(pet);

        // Act
        var result = await _petService.CreatePetAsync(createPetDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedPetDto.Id, result.Id);
        Assert.Equal(expectedPetDto.Name, result.Name);
        Assert.Equal(expectedPetDto.Price, result.Price);
        Assert.Equal(expectedPetDto.Kind, result.Kind);
        Assert.Equal(expectedPetDto.Color, result.Color);
        Assert.Equal(expectedPetDto.Breed, result.Breed);
        Assert.Equal(expectedPetDto.AgeInMonths, result.AgeInMonths);
        Assert.Equal(expectedPetDto.Description, result.Description);

        _mockMapper.Verify(m => m.ToDomain(createPetDto), Times.Once);
        _mockMapper.Verify(m => m.ToDto(pet), Times.Once);
        _mockRepository.Verify(r => r.CreatePetAsync(pet), Times.Once);
    }

    [Fact]
    public async Task GetPetAsync_ExistingPet_ReturnsPetDto()
    {
        // Arrange
        var petId = 1;
        var pet = new Pet
        {
            Id = petId,
            Name = "Buddy",
            Price = 500.00m,
            Kind = PetKind.Dog,
            Color = "Golden",
            Breed = "Golden Retriever",
            AgeInMonths = 24,
            Description = "Friendly and energetic dog"
        };

        var expectedPetDto = new PetDto
        {
            Id = petId,
            Name = "Buddy",
            Price = 500.00m,
            Kind = PetKind.Dog,
            Color = "Golden",
            Breed = "Golden Retriever",
            AgeInMonths = 24,
            Description = "Friendly and energetic dog"
        };

        _mockRepository.Setup(r => r.GetPetByIdAsync(petId)).ReturnsAsync(pet);
        _mockMapper.Setup(m => m.ToDto(pet)).Returns(expectedPetDto);

        // Act
        var result = await _petService.GetPetAsync(petId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedPetDto.Id, result.Id);
        Assert.Equal(expectedPetDto.Name, result.Name);
        Assert.Equal(expectedPetDto.Price, result.Price);
        Assert.Equal(expectedPetDto.Kind, result.Kind);

        _mockRepository.Verify(r => r.GetPetByIdAsync(petId), Times.Once);
        _mockMapper.Verify(m => m.ToDto(pet), Times.Once);
    }

    [Fact]
    public async Task GetPetAsync_NonExistingPet_ReturnsNull()
    {
        // Arrange
        var petId = 999;
        _mockRepository.Setup(r => r.GetPetByIdAsync(petId)).ReturnsAsync((Pet?)null);

        // Act
        var result = await _petService.GetPetAsync(petId);

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(r => r.GetPetByIdAsync(petId), Times.Once);
        _mockMapper.Verify(m => m.ToDto(It.IsAny<Pet>()), Times.Never);
    }

    [Fact]
    public async Task GetAllPetsAsync_ReturnsAllPets()
    {
        // Arrange
        var pets = new List<Pet>
        {
            new Pet { Id = 1, Name = "Buddy", Price = 500.00m, Kind = PetKind.Dog, Color = "Golden", Breed = "Golden Retriever" },
            new Pet { Id = 2, Name = "Whiskers", Price = 200.00m, Kind = PetKind.Cat, Color = "Orange", Breed = "Persian" }
        };

        var expectedPetDtos = new List<PetDto>
        {
            new PetDto { Id = 1, Name = "Buddy", Price = 500.00m, Kind = PetKind.Dog, Color = "Golden", Breed = "Golden Retriever" },
            new PetDto { Id = 2, Name = "Whiskers", Price = 200.00m, Kind = PetKind.Cat, Color = "Orange", Breed = "Persian" }
        };

        _mockRepository.Setup(r => r.GetAllPetsAsync()).ReturnsAsync(pets);
        _mockMapper.Setup(m => m.ToDto(pets[0])).Returns(expectedPetDtos[0]);
        _mockMapper.Setup(m => m.ToDto(pets[1])).Returns(expectedPetDtos[1]);

        // Act
        var result = await _petService.GetAllPetsAsync();

        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.Equal(expectedPetDtos[0].Id, resultList[0].Id);
        Assert.Equal(expectedPetDtos[1].Id, resultList[1].Id);

        _mockRepository.Verify(r => r.GetAllPetsAsync(), Times.Once);
        _mockMapper.Verify(m => m.ToDto(It.IsAny<Pet>()), Times.Exactly(2));
    }

    [Fact]
    public async Task UpdatePetAsync_ExistingPet_ReturnsUpdatedPetDto()
    {
        // Arrange
        var petId = 1;
        var updatePetDto = new UpdatePetDto
        {
            Name = "Buddy Updated",
            Price = 600.00m,
            Kind = PetKind.Dog,
            Color = "Golden",
            Breed = "Golden Retriever",
            AgeInMonths = 30,
            Description = "Updated description"
        };

        var pet = new Pet
        {
            Id = petId,
            Name = "Buddy Updated",
            Price = 600.00m,
            Kind = PetKind.Dog,
            Color = "Golden",
            Breed = "Golden Retriever",
            AgeInMonths = 30,
            Description = "Updated description"
        };

        var expectedPetDto = new PetDto
        {
            Id = petId,
            Name = "Buddy Updated",
            Price = 600.00m,
            Kind = PetKind.Dog,
            Color = "Golden",
            Breed = "Golden Retriever",
            AgeInMonths = 30,
            Description = "Updated description"
        };

        _mockMapper.Setup(m => m.ToDomain(updatePetDto)).Returns(pet);
        _mockRepository.Setup(r => r.UpdatePetAsync(pet)).ReturnsAsync(pet);
        _mockMapper.Setup(m => m.ToDto(pet)).Returns(expectedPetDto);

        // Act
        var result = await _petService.UpdatePetAsync(petId, updatePetDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedPetDto.Id, result.Id);
        Assert.Equal(expectedPetDto.Name, result.Name);
        Assert.Equal(expectedPetDto.Price, result.Price);
        Assert.Equal(expectedPetDto.Kind, result.Kind);

        _mockMapper.Verify(m => m.ToDomain(updatePetDto), Times.Once);
        _mockRepository.Verify(r => r.UpdatePetAsync(pet), Times.Once);
        _mockMapper.Verify(m => m.ToDto(pet), Times.Once);
    }

    [Fact]
    public async Task UpdatePetAsync_NonExistingPet_ReturnsNull()
    {
        // Arrange
        var petId = 999;
        var updatePetDto = new UpdatePetDto
        {
            Name = "Buddy",
            Price = 500.00m,
            Kind = PetKind.Dog,
            Color = "Golden",
            Breed = "Golden Retriever",
            AgeInMonths = 24,
            Description = "Friendly dog"
        };

        var pet = new Pet
        {
            Id = petId,
            Name = "Buddy",
            Price = 500.00m,
            Kind = PetKind.Dog,
            Color = "Golden",
            Breed = "Golden Retriever",
            AgeInMonths = 24,
            Description = "Friendly dog"
        };

        _mockMapper.Setup(m => m.ToDomain(updatePetDto)).Returns(pet);
        _mockRepository.Setup(r => r.UpdatePetAsync(pet)).ReturnsAsync((Pet?)null);

        // Act
        var result = await _petService.UpdatePetAsync(petId, updatePetDto);

        // Assert
        Assert.Null(result);
        _mockMapper.Verify(m => m.ToDomain(updatePetDto), Times.Once);
        _mockRepository.Verify(r => r.UpdatePetAsync(pet), Times.Once);
        _mockMapper.Verify(m => m.ToDto(It.IsAny<Pet>()), Times.Never);
    }

    [Fact]
    public async Task DeletePetAsync_ExistingPet_ReturnsDeletedPetDto()
    {
        // Arrange
        var petId = 1;
        var pet = new Pet
        {
            Id = petId,
            Name = "Buddy",
            Price = 500.00m,
            Kind = PetKind.Dog,
            Color = "Golden",
            Breed = "Golden Retriever",
            AgeInMonths = 24,
            Description = "Friendly dog"
        };

        var expectedPetDto = new PetDto
        {
            Id = petId,
            Name = "Buddy",
            Price = 500.00m,
            Kind = PetKind.Dog,
            Color = "Golden",
            Breed = "Golden Retriever",
            AgeInMonths = 24,
            Description = "Friendly dog"
        };

        _mockRepository.Setup(r => r.DeletePetByIdAsync(petId)).ReturnsAsync(pet);
        _mockMapper.Setup(m => m.ToDto(pet)).Returns(expectedPetDto);

        // Act
        var result = await _petService.DeletePetAsync(petId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedPetDto.Id, result.Id);
        Assert.Equal(expectedPetDto.Name, result.Name);
        Assert.Equal(expectedPetDto.Price, result.Price);
        Assert.Equal(expectedPetDto.Kind, result.Kind);

        _mockRepository.Verify(r => r.DeletePetByIdAsync(petId), Times.Once);
        _mockMapper.Verify(m => m.ToDto(pet), Times.Once);
    }

    [Fact]
    public async Task DeletePetAsync_NonExistingPet_ReturnsNull()
    {
        // Arrange
        var petId = 999;
        _mockRepository.Setup(r => r.DeletePetByIdAsync(petId)).ReturnsAsync((Pet?)null);

        // Act
        var result = await _petService.DeletePetAsync(petId);

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(r => r.DeletePetByIdAsync(petId), Times.Once);
        _mockMapper.Verify(m => m.ToDto(It.IsAny<Pet>()), Times.Never);
    }
} 