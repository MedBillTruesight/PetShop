using PetShop.Application.DTOs;
using PetShop.Domain.Enums;
using PetShop.Domain.Exceptions;

namespace PetShop.Tests.IntegrationTests;

public class PetServiceTests : IntegrationTestBase
{

    [Fact]
    public async Task CreatePetAsync_ShouldCreatePetSuccessfully()
    {
        // Arrange
        var newPet = new CreatePetDto()
        {
            Name = "Buddy",
            Kind = PetKind.Bird,
            Breed = "Golden Retriever",
            AgeInMonths = 10,
            Color = "Yellow",
            Description = "A friendly bird",
            IsVaccinated = true,
            Price = 500.00m
        };
        // Act
        var createdPet = await _petService.CreatePetAsync(newPet);

        // Assert
        Assert.NotNull(createdPet);
        Assert.Equal(newPet.Name, createdPet.Name);
        Assert.Equal(newPet.Kind, createdPet.Kind);
        Assert.Equal(newPet.Breed, createdPet.Breed);
        Assert.Equal(newPet.AgeInMonths, createdPet.AgeInMonths);
        Assert.Equal(newPet.Color, createdPet.Color);
        Assert.Equal(newPet.Description, createdPet.Description);
        Assert.Equal(newPet.IsVaccinated, createdPet.IsVaccinated);
        Assert.Equal(newPet.Price, createdPet.Price);
    }

    [Fact]
    public async Task GetPetAsync_ShouldReturnPet_WhenPetExists()
    {
        // Arrange
        var testPet = await CreatePetTestAsync();

        // Act
        var fetchedPet = await _petService.GetPetAsync(testPet.Id);

        // Assert
        Assert.NotNull(fetchedPet);
        Assert.Equal(testPet.Id, fetchedPet.Id);
        Assert.Equal(testPet.Name, fetchedPet.Name);
        Assert.Equal(testPet.Kind, fetchedPet.Kind);
        Assert.Equal(testPet.Breed, fetchedPet.Breed);
        Assert.Equal(testPet.AgeInMonths, fetchedPet.AgeInMonths);
        Assert.Equal(testPet.Color, fetchedPet.Color);
        Assert.Equal(testPet.Description, fetchedPet.Description);
        Assert.Equal(testPet.IsVaccinated, fetchedPet.IsVaccinated);
        Assert.Equal(testPet.Price, fetchedPet.Price);
    }

    [Fact]
    public async Task GetAllPetsAsync_ShouldReturnAllPets()
    {
        // Arrange
        var newPet = new CreatePetDto()
        {
            Name = "Whiskers",
            Kind = PetKind.Bird,
            Breed = "Golden Retriever",
            AgeInMonths = 10,
            Color = "Yellow",
            Description = "A friendly bird",
            IsVaccinated = true,
            Price = 500.00m
        };
        // Act
        var testPet1 = await _petService.CreatePetAsync(newPet);
        var testPet2 = await CreatePetTestAsync();

        // Act
        var allPets = await _petService.GetAllPetsAsync();
        // Assert
        Assert.NotNull(allPets);
        var allPetsList = allPets.ToList();
        Assert.True(allPetsList.Count() >= 2);
        Assert.Contains(allPetsList, p => p.Id == testPet1.Id);
        Assert.Contains(allPetsList, p => p.Id == testPet2.Id);
    }

    [Fact]
    public async Task DeletePetAsync_ShouldDeletePetSuccessfully()
    {
        // Arrange
        var testPet = await CreatePetTestAsync();
        // Act
        var deleteResult = await _petService.DeletePetAsync(testPet.Id);
        // Assert
        Assert.NotNull(deleteResult);
        Assert.Equal(testPet.Id, deleteResult.Id);
    }
    
    [Fact]
    public async Task DeletePetAsync_ShouldThrowException_WhenPetDoesNotExist()
    {
        // Arrange
        var nonExistentPetId = Guid.NewGuid(); 
        
        // Act + Assert
        var ex = await Assert.ThrowsAsync<AppException>(() => _petService.DeletePetAsync(nonExistentPetId));
        Assert.Equal($"Pet with id '{nonExistentPetId}' not found.", ex.Message);
    }
    
    [Fact]
    public async Task CreatePetAsync_ShouldThrowException_WhenPetNameIsNotUnique()
    {
        // Arrange
        var petName = "UniquePetName";
        var firstPet = new CreatePetDto()
        {
            Name = petName,
            Kind = PetKind.Cat,
            Breed = "Siamese",
            AgeInMonths = 12,
            Color = "Cream",
            Description = "A friendly cat",
            IsVaccinated = true,
            Price = 300.00m
        };
        
        var secondPet = new CreatePetDto()
        {
            Name = petName, // Same name as firstPet
            Kind = PetKind.Dog,
            Breed = "Beagle",
            AgeInMonths = 8,
            Color = "Brown",
            Description = "A playful dog",
            IsVaccinated = false,
            Price = 400.00m
        };
        
        // Act
        await _petService.CreatePetAsync(firstPet); 
        
        // Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _petService.CreatePetAsync(secondPet));
        Assert.Equal($"A pet with the name '{petName}' already exists.", ex.Message);
    }

    [Fact]
    public async Task UpdatePetAsync_ShouldUpdatePetSuccessfully()
    {
        // Arrange
        var testPet = await CreatePetTestAsync();
        var updatePetDto = new UpdatePetDto()
        {
            Name = "UpdatedName",
            Kind = PetKind.Cat,
            Breed = "Persian",
            AgeInMonths = 36,
            Color = "White",
            Description = "An updated description",
            IsVaccinated = true,
            Price = 600.00m
        };

        // Act
        var updatedPet = await _petService.UpdatePetAsync(testPet.Id, updatePetDto);
        // Assert
        Assert.NotNull(updatedPet);
        Assert.Equal(testPet.Id, updatedPet.Id);
        Assert.Equal(updatePetDto.Name, updatedPet.Name);
        Assert.Equal(updatePetDto.Kind, updatedPet.Kind);
        Assert.Equal(updatePetDto.Breed, updatedPet.Breed);
        Assert.Equal(updatePetDto.AgeInMonths, updatedPet.AgeInMonths);
        Assert.Equal(updatePetDto.Color, updatedPet.Color);
        Assert.Equal(updatePetDto.Description, updatedPet.Description);
        Assert.Equal(updatePetDto.IsVaccinated, updatedPet.IsVaccinated);
        Assert.Equal(updatePetDto.Price, updatedPet.Price);
    }
    
    [Fact]
    public async Task UpdatePetAsync_ShouldThrowException_WhenPetDoesNotExist()
    {
        // Arrange
        var nonExistentPetId = Guid.NewGuid();
        var updatePetDto = new UpdatePetDto()
        {
            Name = "NonExistentPet",
            Kind = PetKind.Cat,
            Breed = "Persian",
            AgeInMonths = 36,
            Color = "White",
            Description = "An updated description",
            IsVaccinated = true,
            Price = 600.00m
        };  
        
        
        // Act & Assert
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _petService.UpdatePetAsync(nonExistentPetId, updatePetDto));

        Assert.Equal($"Pet not found for update", ex.Message);
    }
    
    [Fact]
    public async Task GetPetAsync_ShouldThrowException_WhenPetDoesNotExist()
    {
        // Arrange
        var nonExistentPetId = Guid.NewGuid();
        
        // Act + Assert
        var ex = await Assert.ThrowsAsync<AppException>(() => _petService.GetPetAsync(nonExistentPetId));
        Assert.Equal($"Pet with id '{nonExistentPetId}' not found.", ex.Message);
    }

    [Fact]
    public async Task GetAllPetsAsync_ShouldReturnEmptyList_WhenNoPetsExist()
    {
        // Arrange
        // Ensure the database is empty
        // Act
        var allPets = await _petService.GetAllPetsAsync();

        // Assert
        Assert.NotNull(allPets);
        Assert.Empty(allPets);

    }

}