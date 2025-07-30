using PetShop.Application.DTOs;
using PetShop.Domain.Enums;
using Xunit;

namespace PetShop.Test;

public class PetServiceIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task CreatePetAsync_ValidData_CreatesPetInDatabase()
    {
        // Arrange
        var createPetDto = new CreatePetDto
        {
            Name = "Whiskers",
            Price = 300.00m,
            Kind = PetKind.Cat,
            Color = "Orange",
            Breed = "Persian",
            AgeInMonths = 18,
            Description = "Calm and friendly cat"
        };

        // Act
        var result = await _petService.CreatePetAsync(createPetDto);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(0, result.Id);
        Assert.Equal(createPetDto.Name, result.Name);
        Assert.Equal(createPetDto.Price, result.Price);
        Assert.Equal(createPetDto.Kind, result.Kind);
        Assert.Equal(createPetDto.Color, result.Color);
        Assert.Equal(createPetDto.Breed, result.Breed);
        Assert.Equal(createPetDto.AgeInMonths, result.AgeInMonths);
        Assert.Equal(createPetDto.Description, result.Description);

        // Verify pet was saved to database
        var savedPet = await _petService.GetPetAsync(result.Id);
        Assert.NotNull(savedPet);
        Assert.Equal(result.Id, savedPet.Id);
        Assert.Equal(result.Name, savedPet.Name);
    }

    [Fact]
    public async Task GetPetAsync_ExistingPet_ReturnsPet()
    {
        // Arrange
        var pet = await CreateTestPetAsync();

        // Act
        var result = await _petService.GetPetAsync(pet.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(pet.Id, result.Id);
        Assert.Equal(pet.Name, result.Name);
        Assert.Equal(pet.Price, result.Price);
        Assert.Equal(pet.Kind, result.Kind);
    }

    [Fact]
    public async Task GetPetAsync_NonExistingPet_ReturnsNull()
    {
        // Act
        var result = await _petService.GetPetAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllPetsAsync_ReturnsAllPets()
    {
        // Arrange
        var pet1 = await CreateTestPetAsync();
        var pet2 = await CreateTestPetAsync();

        // Act
        var result = await _petService.GetAllPetsAsync();

        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();
        Assert.True(resultList.Count >= 2);
        Assert.Contains(resultList, p => p.Id == pet1.Id);
        Assert.Contains(resultList, p => p.Id == pet2.Id);
    }

    [Fact]
    public async Task UpdatePetAsync_ExistingPet_UpdatesPetInDatabase()
    {
        // Arrange
        var pet = await CreateTestPetAsync();
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

        // Act
        var result = await _petService.UpdatePetAsync(pet.Id, updatePetDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(pet.Id, result.Id);
        Assert.Equal(updatePetDto.Name, result.Name);
        Assert.Equal(updatePetDto.Price, result.Price);
        Assert.Equal(updatePetDto.Kind, result.Kind);
        Assert.Equal(updatePetDto.Color, result.Color);
        Assert.Equal(updatePetDto.Breed, result.Breed);
        Assert.Equal(updatePetDto.AgeInMonths, result.AgeInMonths);
        Assert.Equal(updatePetDto.Description, result.Description);

        // Verify pet was updated in database
        var updatedPet = await _petService.GetPetAsync(pet.Id);
        Assert.NotNull(updatedPet);
        Assert.Equal(updatePetDto.Name, updatedPet.Name);
        Assert.Equal(updatePetDto.Price, updatedPet.Price);
    }

    [Fact]
    public async Task UpdatePetAsync_NonExistingPet_ReturnsNull()
    {
        // Arrange
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

        // Act
        var result = await _petService.UpdatePetAsync(999, updatePetDto);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeletePetAsync_ExistingPet_DeletesPetFromDatabase()
    {
        // Arrange
        var pet = await CreateTestPetAsync();

        // Act
        var result = await _petService.DeletePetAsync(pet.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(pet.Id, result.Id);
        Assert.Equal(pet.Name, result.Name);
        Assert.Equal(pet.Price, result.Price);
        Assert.Equal(pet.Kind, result.Kind);

        // Verify pet was deleted from database
        var deletedPet = await _petService.GetPetAsync(pet.Id);
        Assert.Null(deletedPet);
    }

    [Fact]
    public async Task DeletePetAsync_NonExistingPet_ReturnsNull()
    {
        // Act
        var result = await _petService.DeletePetAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task PetCRUD_CompleteWorkflow_Succeeds()
    {
        // Create
        var createPetDto = new CreatePetDto
        {
            Name = "Fluffy",
            Price = 400.00m,
            Kind = PetKind.Cat,
            Color = "White",
            Breed = "Maine Coon",
            AgeInMonths = 12,
            Description = "Large and fluffy cat"
        };

        var createdPet = await _petService.CreatePetAsync(createPetDto);
        Assert.NotNull(createdPet);
        Assert.NotEqual(0, createdPet.Id);

        // Read
        var retrievedPet = await _petService.GetPetAsync(createdPet.Id);
        Assert.NotNull(retrievedPet);
        Assert.Equal(createdPet.Id, retrievedPet.Id);
        Assert.Equal(createPetDto.Name, retrievedPet.Name);

        // Update
        var updatePetDto = new UpdatePetDto
        {
            Name = "Fluffy Updated",
            Price = 450.00m,
            Kind = PetKind.Cat,
            Color = "White",
            Breed = "Maine Coon",
            AgeInMonths = 15,
            Description = "Updated description"
        };

        var updatedPet = await _petService.UpdatePetAsync(createdPet.Id, updatePetDto);
        Assert.NotNull(updatedPet);
        Assert.Equal(updatePetDto.Name, updatedPet.Name);
        Assert.Equal(updatePetDto.Price, updatedPet.Price);

        // Verify update
        var verifyUpdatedPet = await _petService.GetPetAsync(createdPet.Id);
        Assert.NotNull(verifyUpdatedPet);
        Assert.Equal(updatePetDto.Name, verifyUpdatedPet.Name);

        // Delete
        var deletedPet = await _petService.DeletePetAsync(createdPet.Id);
        Assert.NotNull(deletedPet);
        Assert.Equal(createdPet.Id, deletedPet.Id);

        // Verify deletion
        var verifyDeletedPet = await _petService.GetPetAsync(createdPet.Id);
        Assert.Null(verifyDeletedPet);
    }

    [Fact]
    public async Task GetAllPetsAsync_DifferentPetTypes_ReturnsCorrectPets()
    {
        // Arrange
        var dogPet = new CreatePetDto
        {
            Name = "Rex",
            Price = 800.00m,
            Kind = PetKind.Dog,
            Color = "Black",
            Breed = "German Shepherd",
            AgeInMonths = 36,
            Description = "Loyal and protective dog"
        };

        var catPet = new CreatePetDto
        {
            Name = "Mittens",
            Price = 250.00m,
            Kind = PetKind.Cat,
            Color = "Gray",
            Breed = "Russian Blue",
            AgeInMonths = 24,
            Description = "Elegant and quiet cat"
        };

        var createdDog = await _petService.CreatePetAsync(dogPet);
        var createdCat = await _petService.CreatePetAsync(catPet);

        // Act
        var result = await _petService.GetAllPetsAsync();

        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();
        Assert.True(resultList.Count >= 2);

        var foundDog = resultList.FirstOrDefault(p => p.Id == createdDog.Id);
        var foundCat = resultList.FirstOrDefault(p => p.Id == createdCat.Id);

        Assert.NotNull(foundDog);
        Assert.NotNull(foundCat);
        Assert.Equal(PetKind.Dog, foundDog.Kind);
        Assert.Equal(PetKind.Cat, foundCat.Kind);
        Assert.Equal("Rex", foundDog.Name);
        Assert.Equal("Mittens", foundCat.Name);
    }
} 