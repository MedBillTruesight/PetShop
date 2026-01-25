using PetShop.Application.DTOs;
using PetShop.Application.Interfaces.Mappers;
using PetShop.Application.Interfaces.Services;
using PetShop.Domain.Exceptions;
using PetShop.Domain.Interfaces.Repositories;

namespace PetShop.Application.Features.Pets;
public class PetService : IPetService
{
	private readonly IPetRepository _petRepository;
	private readonly IPetMapper _petMapper;

	public PetService(IPetRepository petRepository, IPetMapper petMapper)
	{
		_petRepository = petRepository;
		_petMapper = petMapper;
	}

	public async Task<PetDto> CreatePetAsync(CreatePetDto createPetDto)
	{
		// Map and create (validation handled by FluentValidation middleware)
		var isNameUnique = await ValidateUniqueName(createPetDto.Name);
		if (!isNameUnique)
			throw new InvalidOperationException($"A pet with the name '{createPetDto.Name}' already exists.");
			
		var pet = _petMapper.ToDomain(createPetDto);
		await _petRepository.CreatePetAsync(pet);

		if (pet.Id == Guid.Empty)
			throw new InvalidOperationException("Failed to create pet: ID was not generated.");

		return _petMapper.ToDto(pet);
	}

	public async Task<PetDto?> GetPetAsync(Guid id)
	{
		var pet = await _petRepository.GetPetByIdAsync(id);
		if (pet == null) throw new AppException($"Pet with id '{id}' not found.");
		return _petMapper.ToDto(pet);
	}

	public async Task<IEnumerable<PetDto>> GetAllPetsAsync()
	{
		var pets = await _petRepository.GetAllPetsAsync();

		return pets.Select(_petMapper.ToDto);
	}

	public async Task<PetDto?> UpdatePetAsync(Guid id, UpdatePetDto updatePetDto)
	{
		var pet =  _petMapper.ToDomain(updatePetDto);
		pet.Id = id;

		var updatedPet = await _petRepository.UpdatePetAsync(pet);
		if (updatedPet == null)
			throw new AppException($"Pet with id '{id}' not found.");
		return _petMapper.ToDto(updatedPet!);
	}

	public async Task<PetDto?> DeletePetAsync(Guid id)
	{
		var deletedPet = await _petRepository.DeletePetByIdAsync(id);
		if (deletedPet == null) throw new AppException($"Pet with id '{id}' not found.");

		return _petMapper.ToDto(deletedPet);
	}

	private async Task<bool> ValidateUniqueName(string name)
	{
		var existingPets = await _petRepository.GetAllPetsAsync();
		return !existingPets.Any(p => p.Name!.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase));
	}
}