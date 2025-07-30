
using PetShop.Application.DTOs;
using PetShop.Application.Features.Pet.Interfaces;
using PetShop.Application.Mappers;
using PetShop.Domain.Interfaces;

namespace PetShop.Application.Features.Pet.Services;

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
		Domain.Entities.Pet pet = _petMapper.ToDomain(createPetDto);

		await _petRepository.CreatePetAsync(pet);

		return _petMapper.ToDto(pet);
	}

	public async Task<PetDto?> GetPetAsync(int id)
	{
		var pet = await _petRepository.GetPetByIdAsync(id);
		if (pet == null) return null;
		return _petMapper.ToDto(pet);
	}

	public async Task<IEnumerable<PetDto>> GetAllPetsAsync()
	{
		var pets = await _petRepository.GetAllPetsAsync();

		return pets.Select(_petMapper.ToDto);
	}

	public async Task<PetDto?> UpdatePetAsync(int id, UpdatePetDto updatePetDto)
	{
		var pet =  _petMapper.ToDomain(updatePetDto);
		pet.Id = id;

		var updatedPet = await _petRepository.UpdatePetAsync(pet);
		if (updatedPet == null)
		{
			return null;
		}

		return _petMapper.ToDto(updatedPet);
	}

	public async Task<PetDto?> DeletePetAsync(int id)
	{
		var deletedPet = await _petRepository.DeletePetByIdAsync(id);
		if (deletedPet == null) return null;

		return _petMapper.ToDto(deletedPet);
	}
}