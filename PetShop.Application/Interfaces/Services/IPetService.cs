

using PetShop.Application.DTOs;

namespace PetShop.Application.Interfaces.Services;

public interface IPetService
{
	Task<PetDto> CreatePetAsync(CreatePetDto createPetDto);
	Task<PetDto?> GetPetAsync(Guid id);
	Task<IEnumerable<PetDto>> GetAllPetsAsync();
	Task<PetDto?> UpdatePetAsync(Guid id, UpdatePetDto updatePetDto);
	Task<PetDto?> DeletePetAsync(Guid id);
}