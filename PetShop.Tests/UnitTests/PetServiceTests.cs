using Moq;
using PetShop.Application.DTOs;
using PetShop.Application.Features.Pets;
using PetShop.Application.Interfaces.Mappers;
using PetShop.Domain.Entities;
using PetShop.Domain.Enums;
using PetShop.Domain.Exceptions;
using PetShop.Domain.Interfaces.Repositories;

namespace PetShop.Tests.UnitTests;

public class PetServiceTests
{
    private readonly PetService _petService;
    private readonly Mock<IPetRepository> _mockRepository;
    private readonly Mock<IPetMapper> _petMapper;

    public PetServiceTests()
    {
        _mockRepository = new Mock<IPetRepository>();
        _petMapper = new Mock<IPetMapper>();
        _petService = new PetService(_mockRepository.Object, _petMapper.Object);
    }

    [Fact]
    public async Task GetPetAsync_PetExists_ReturnsPetDto()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var createPetDtoDto = new CreatePetDto()
        {
            Kind = PetKind.Bird,
            Name = "Buddy",
            Breed = "Golden Retriever",
            AgeInMonths = 10,
            Color = "Yellow",
            Description = "A friendly bird",
            IsVaccinated = true,
            Price = 500.00m
        };

        //create a pet domain object to be returned by the repository
        var petDomain = new Pet()
        {
            Id = petId,
            Kind = createPetDtoDto.Kind,
            Name = createPetDtoDto.Name,
            Breed = createPetDtoDto.Breed,
            AgeInMonths = createPetDtoDto.AgeInMonths,
            Color = createPetDtoDto.Color,
            Description = createPetDtoDto.Description,
            IsVaccinated = createPetDtoDto.IsVaccinated,
            Price = createPetDtoDto.Price
        };
        
        //expected petdto to be returned by the service
        var expectedPetDto = new PetDto()
        {
            Id = petId,
            Kind = createPetDtoDto.Kind,
            Name = createPetDtoDto.Name,
            Breed = createPetDtoDto.Breed,
            AgeInMonths = createPetDtoDto.AgeInMonths,
            Color = createPetDtoDto.Color,
            Description = createPetDtoDto.Description,
            IsVaccinated = createPetDtoDto.IsVaccinated,
            Price = createPetDtoDto.Price
        };
        
       //create 
       _petMapper.Setup(m => m.ToDomain(createPetDtoDto)).Returns(petDomain);
       _petMapper.Setup(m => m.ToDto(petDomain)).Returns(expectedPetDto);
       _mockRepository.Setup(r => r.GetAllPetsAsync()).ReturnsAsync(new List<Pet>());
       _mockRepository.Setup(r => r.CreatePetAsync(petDomain)).ReturnsAsync(petDomain);
       
        // Act
        var createdPet = await _petService.CreatePetAsync(createPetDtoDto);

        // Assert
        Assert.NotNull(createdPet);
        Assert.Equal(expectedPetDto.Id, createdPet.Id);
        Assert.Equal(expectedPetDto.Name, createdPet.Name);
        Assert.Equal(expectedPetDto.Kind, createdPet.Kind);
        Assert.Equal(expectedPetDto.Breed, createdPet.Breed);
        Assert.Equal(expectedPetDto.AgeInMonths, createdPet.AgeInMonths);
        Assert.Equal(expectedPetDto.Color, createdPet.Color);
        Assert.Equal(expectedPetDto.Description, createdPet.Description);
        Assert.Equal(expectedPetDto.IsVaccinated, createdPet.IsVaccinated);
        Assert.Equal(expectedPetDto.Price, createdPet.Price);
        
        _petMapper.Verify(m => m.ToDomain(createPetDtoDto), Times.Once);
        _petMapper.Verify(m => m.ToDto(petDomain), Times.Once);
        _mockRepository.Verify(m => m.CreatePetAsync(petDomain), Times.Once);
    }

    [Fact]
    public async Task GetPetAsync_PetDoesNotExist_ThrowsAppException()
    {
        // Arrange
        var petId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetPetByIdAsync(petId)).ReturnsAsync((Pet?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _petService.GetPetAsync(petId));
        Assert.Equal($"Pet with id '{petId}' not found.", exception.Message);
        
        _mockRepository.Verify(r => r.GetPetByIdAsync(petId), Times.Once);
        
    }

    [Fact]
    public async Task GetAllPetsAsync_NoPetsExist_ReturnsEmptyList()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllPetsAsync()).ReturnsAsync(new List<Pet>());

        // Act
        var pets = await _petService.GetAllPetsAsync();

        // Assert
        Assert.NotNull(pets);
        Assert.Empty(pets);
        _mockRepository.Verify(r => r.GetAllPetsAsync(), Times.Once);

    }

    [Fact]
    public async Task GetAllPetsAsync_PetsExist_ReturnsListOfPetDtos()
    {
        // Arrange
        var pet1 = new Pet()
        {
            Id = Guid.NewGuid(),
            Kind = PetKind.Dog,
            Name = "Max",
            Breed = "Labrador",
            AgeInMonths = 24,
            Color = "Black",
            Description = "A playful dog",
            IsVaccinated = true,
            Price = 800.00m
        };

        var pet2 = new Pet()
        {
            Id = Guid.NewGuid(),
            Kind = PetKind.Cat,
            Name = "Whiskers",
            Breed = "Siamese",
            AgeInMonths = 12,
            Color = "Cream",
            Description = "A curious cat",
            IsVaccinated = false,
            Price = 600.00m
        };

        var petsList = new List<Pet> { pet1, pet2 };
        _mockRepository.Setup(r => r.GetAllPetsAsync()).ReturnsAsync(petsList);

        _petMapper.Setup(m => m.ToDto(pet1)).Returns(new PetDto
        {
            Id = pet1.Id,
            Kind = pet1.Kind,
            Name = pet1.Name,
            Breed = pet1.Breed,
            AgeInMonths = pet1.AgeInMonths,
            Color = pet1.Color,
            Description = pet1.Description,
            IsVaccinated = pet1.IsVaccinated,
            Price = pet1.Price
        });

        _petMapper.Setup(m => m.ToDto(pet2)).Returns(new PetDto
        {
            Id = pet2.Id,
            Kind = pet2.Kind,
            Name = pet2.Name,
            Breed = pet2.Breed,
            AgeInMonths = pet2.AgeInMonths,
            Color = pet2.Color,
            Description = pet2.Description,
            IsVaccinated = pet2.IsVaccinated,
            Price = pet2.Price
        });

        // Act
        var pets = await _petService.GetAllPetsAsync();

        // Assert
        Assert.NotNull(pets);
        var petDtos = pets.ToList();
        Assert.Equal(2, petDtos.Count);
        Assert.Contains(petDtos, p => p.Id == pet1.Id);
        Assert.Contains(petDtos, p => p.Id == pet2.Id);

        _mockRepository.Verify(r => r.GetAllPetsAsync(), Times.Once);
        _petMapper.Verify(m => m.ToDto(pet1), Times.Once);
        _petMapper.Verify(m => m.ToDto(pet2), Times.Once);

    }

    [Fact]
    public async Task DeletePetAsync_PetExists_ReturnsDeletedPetDto()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var petDomain = new Pet()
        {
            Id = petId,
            Kind = PetKind.Dog,
            Name = "Rex",
            Breed = "German Shepherd",
            AgeInMonths = 36,
            Color = "Brown",
            Description = "A loyal dog",
            IsVaccinated = true,
            Price = 900.00m
        };

        var expectedPetDto = new PetDto()
        {
            Id = petId,
            Kind = petDomain.Kind,
            Name = petDomain.Name,
            Breed = petDomain.Breed,
            AgeInMonths = petDomain.AgeInMonths,
            Color = petDomain.Color,
            Description = petDomain.Description,
            IsVaccinated = petDomain.IsVaccinated,
            Price = petDomain.Price
        };

        _mockRepository.Setup(r => r.DeletePetByIdAsync(petId)).ReturnsAsync(petDomain);
        _petMapper.Setup(m => m.ToDto(petDomain)).Returns(expectedPetDto);

        // Act
        var deletedPet = await _petService.DeletePetAsync(petId);

        // Assert
        Assert.NotNull(deletedPet);
        Assert.Equal(expectedPetDto.Id, deletedPet.Id);
        Assert.Equal(expectedPetDto.Name, deletedPet.Name);
        Assert.Equal(expectedPetDto.Kind, deletedPet.Kind);
        Assert.Equal(expectedPetDto.Breed, deletedPet.Breed);
        Assert.Equal(expectedPetDto.AgeInMonths, deletedPet.AgeInMonths);
        Assert.Equal(expectedPetDto.Color, deletedPet.Color);
        Assert.Equal(expectedPetDto.Description, deletedPet.Description);
        Assert.Equal(expectedPetDto.IsVaccinated, deletedPet.IsVaccinated);
        Assert.Equal(expectedPetDto.Price, deletedPet.Price);

        _mockRepository.Verify(r => r.DeletePetByIdAsync(petId), Times.Once);
        _petMapper.Verify(m => m.ToDto(petDomain), Times.Once);

    }

    [Fact]
    public async Task DeletePetAsync_PetDoesNotExist_ThrowsAppException()
    {
        // Arrange
        var petId = Guid.NewGuid();
        _mockRepository.Setup(r => r.DeletePetByIdAsync(petId)).ReturnsAsync((Pet?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _petService.DeletePetAsync(petId));
        Assert.Equal($"Pet with id '{petId}' not found.", exception.Message);

        _mockRepository.Verify(r => r.DeletePetByIdAsync(petId), Times.Once);
    }

    [Fact]
    public async Task CreatePetAsync_DuplicateName_ThrowsInvalidOperationException()
    {
        // Arrange
        var petName = "Fluffy";
        var createPetDto = new CreatePetDto()
        {
            Kind = PetKind.Cat,
            Name = petName,
            Breed = "Siamese",
            AgeInMonths = 12,
            Color = "Cream",
            Description = "A friendly cat",
            IsVaccinated = true,
            Price = 300.00m
        };

        var existingPet = new Pet()
        {
            Id = Guid.NewGuid(),
            Kind = PetKind.Cat,
            Name = petName,
            Breed = "Persian",
            AgeInMonths = 18,
            Color = "White",
            Description = "Another friendly cat",
            IsVaccinated = true,
            Price = 400.00m
        };

        _mockRepository.Setup(r => r.GetAllPetsAsync()).ReturnsAsync(new List<Pet> { existingPet });
        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(() => _petService.CreatePetAsync(createPetDto));
        Assert.Equal($"A pet with the name '{petName}' already exists.", exception.Message);

        _mockRepository.Verify(r => r.GetAllPetsAsync(), Times.Once);
    }

    [Fact]
    public async Task CreatePetAsync_IdNotGenerated_ThrowsInvalidOperationException()
    {
        // Arrange
        var createPetDto = new CreatePetDto()
        {
            Kind = PetKind.Cat,
            Name = "Shadow",
            Breed = "Siamese",
            AgeInMonths = 12,
            Color = "Cream",
            Description = "A friendly cat",
            IsVaccinated = true,
            Price = 300.00m
        };

        var petDomain = new Pet()
        {
            Id = Guid.Empty, // Simulate failure to generate ID
            Kind = createPetDto.Kind,
            Name = createPetDto.Name,
            Breed = createPetDto.Breed,
            AgeInMonths = createPetDto.AgeInMonths,
            Color = createPetDto.Color,
            Description = createPetDto.Description,
            IsVaccinated = createPetDto.IsVaccinated,
            Price = createPetDto.Price
        };

        _petMapper.Setup(m => m.ToDomain(createPetDto)).Returns(petDomain);
        _mockRepository.Setup(r => r.GetAllPetsAsync()).ReturnsAsync(new List<Pet>());
        _mockRepository.Setup(r => r.CreatePetAsync(petDomain)).ReturnsAsync(petDomain);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(() => _petService.CreatePetAsync(createPetDto));
        Assert.Equal("Failed to create pet: ID was not generated.", exception.Message);

        _petMapper.Verify(m => m.ToDomain(createPetDto), Times.Once);
        _mockRepository.Verify(r => r.GetAllPetsAsync(), Times.Once);
        _mockRepository.Verify(r => r.CreatePetAsync(petDomain), Times.Once);

    }

    [Fact]
    public async Task UpdatePetAsync_PetExists_ReturnsUpdatedPetDto()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var updatePetDto = new UpdatePetDto()
        {
            Kind = PetKind.Dog,
            Name = "Buddy",
            Breed = "Beagle",
            AgeInMonths = 18,
            Color = "Brown",
            Description = "A playful dog",
            IsVaccinated = true,
            Price = 700.00m
        };

        var petDomain = new Pet()
        {
            Id = petId,
            Kind = PetKind.Dog,
            Name = updatePetDto.Name,
            Breed = updatePetDto.Breed,
            AgeInMonths = 18,
            Color = updatePetDto.Color,
            Description = updatePetDto.Description,
            IsVaccinated = true,
            Price = 700.00m
        };

        var expectedPetDto = new PetDto()
        {
            Id = petId,
            Kind = PetKind.Dog,
            Name = updatePetDto.Name,
            Breed = updatePetDto.Breed,
            AgeInMonths = 18,
            Color = updatePetDto.Color,
            Description = updatePetDto.Description,
            IsVaccinated = true,
            Price = 700.00m
        };

        _petMapper.Setup(m => m.ToDomain(updatePetDto)).Returns(petDomain);
        _mockRepository.Setup(r => r.UpdatePetAsync(petDomain)).ReturnsAsync(petDomain);
        _petMapper.Setup(m => m.ToDto(petDomain)).Returns(expectedPetDto);

        // Act
        var updatedPet = await _petService.UpdatePetAsync(petId, updatePetDto);

        // Assert
        Assert.NotNull(updatedPet);
        Assert.Equal(expectedPetDto.Id, updatedPet.Id);
        Assert.Equal(expectedPetDto.Name, updatedPet.Name);
        Assert.Equal(expectedPetDto.Kind, updatedPet.Kind);
        Assert.Equal(expectedPetDto.Breed, updatedPet.Breed);
        Assert.Equal(expectedPetDto.AgeInMonths, updatedPet.AgeInMonths);
        Assert.Equal(expectedPetDto.Color, updatedPet.Color);
        Assert.Equal(expectedPetDto.Description, updatedPet.Description);
        Assert.Equal(expectedPetDto.IsVaccinated, updatedPet.IsVaccinated);
        Assert.Equal(expectedPetDto.Price, updatedPet.Price);

        _petMapper.Verify(m => m.ToDomain(updatePetDto), Times.Once);
        _mockRepository.Verify(r => r.UpdatePetAsync(petDomain), Times.Once);
        _petMapper.Verify(m => m.ToDto(petDomain), Times.Once);

    }

    [Fact]
    public async Task UpdatePetAsync_PetDoesNotExist_ThrowsAppException()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var updatePetDto = new UpdatePetDto()
        {
            Kind = PetKind.Dog,
            Name = "Ghost",
            Breed = "Husky",
            AgeInMonths = 20,
            Color = "White",
            Description = "A strong dog",
            IsVaccinated = true,
            Price = 850.00m
        };

        var petDomain = new Pet()
        {
            Id = petId,
            Kind = PetKind.Dog,
            Name = updatePetDto.Name,
            Breed = updatePetDto.Breed,
            AgeInMonths = 20,
            Color = updatePetDto.Color,
            Description = updatePetDto.Description,
            IsVaccinated = true,
            Price = 850.00m
        };

        _petMapper.Setup(m => m.ToDomain(updatePetDto)).Returns(petDomain);
        _mockRepository.Setup(r => r.UpdatePetAsync(petDomain)).ReturnsAsync((Pet?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _petService.UpdatePetAsync(petId, updatePetDto));
        Assert.Equal($"Pet with id '{petId}' not found.", exception.Message);

        _petMapper.Verify(m => m.ToDomain(updatePetDto), Times.Once);
        _mockRepository.Verify(r => r.UpdatePetAsync(petDomain), Times.Once);
    }

    [Fact]
    public async Task CreatePetAsync_ValidPet_ReturnsCreatedPetDto()
    {
        // Arrange
        var createPetDto = new CreatePetDto()
        {
            Kind = PetKind.Cat,
            Name = "Mittens",
            Breed = "Siamese",
            AgeInMonths = 8,
            Color = "Gray",
            Description = "A cute kitten",
            IsVaccinated = true,
            Price = 250.00m
        };

        var petDomain = new Pet()
        {
            Id = Guid.NewGuid(),
            Kind = createPetDto.Kind,
            Name = createPetDto.Name,
            Breed = createPetDto.Breed,
            AgeInMonths = createPetDto.AgeInMonths,
            Color = createPetDto.Color,
            Description = createPetDto.Description,
            IsVaccinated = createPetDto.IsVaccinated,
            Price = createPetDto.Price
        };

        var expectedPetDto = new PetDto()
        {
            Id = petDomain.Id,
            Kind = petDomain.Kind,
            Name = petDomain.Name,
            Breed = petDomain.Breed,
            AgeInMonths = petDomain.AgeInMonths,
            Color = petDomain.Color,
            Description = petDomain.Description,
            IsVaccinated = petDomain.IsVaccinated,
            Price = petDomain.Price
        };

        _petMapper.Setup(m => m.ToDomain(createPetDto)).Returns(petDomain);
        _mockRepository.Setup(r => r.GetAllPetsAsync()).ReturnsAsync(new List<Pet>());
        _mockRepository.Setup(r => r.CreatePetAsync(petDomain)).ReturnsAsync(petDomain);
        _petMapper.Setup(m => m.ToDto(petDomain)).Returns(expectedPetDto);

        // Act
        var createdPet = await _petService.CreatePetAsync(createPetDto);

        // Assert
        Assert.NotNull(createdPet);
        Assert.Equal(expectedPetDto.Id, createdPet.Id);
        Assert.Equal(expectedPetDto.Name, createdPet.Name);
        Assert.Equal(expectedPetDto.Kind, createdPet.Kind);
        Assert.Equal(expectedPetDto.Breed, createdPet.Breed);
        Assert.Equal(expectedPetDto.AgeInMonths, createdPet.AgeInMonths);
        Assert.Equal(expectedPetDto.Color, createdPet.Color);
        Assert.Equal(expectedPetDto.Description, createdPet.Description);
        Assert.Equal(expectedPetDto.IsVaccinated, createdPet.IsVaccinated);
        Assert.Equal(expectedPetDto.Price, createdPet.Price);

        _petMapper.Verify(m => m.ToDomain(createPetDto), Times.Once);
        _mockRepository.Verify(r => r.GetAllPetsAsync(), Times.Once);
        _mockRepository.Verify(r => r.CreatePetAsync(petDomain), Times.Once);
        _petMapper.Verify(m => m.ToDto(petDomain), Times.Once);

    }

}
          