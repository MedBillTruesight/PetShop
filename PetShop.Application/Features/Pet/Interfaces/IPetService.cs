using PetShop.Application.DTOs;

namespace PetShop.Application.Features.Pet.Interfaces;

public interface IPetService
{
	Task<PetDto> CreatePetAsync(CreatePetDto createPetDto);
	
	Task<PetDto?> GetPetAsync(int id);
	Task<IEnumerable<PetDto>> GetAllPetsAsync();
	Task<PetDto?> UpdatePetAsync(int id, UpdatePetDto updatePetDto);
	Task<PetDto?> DeletePetAsync(int id);
}
