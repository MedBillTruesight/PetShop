
using Microsoft.AspNetCore.Mvc;
using PetShop.Application.DTOs;
using PetShop.Application.Features.Customer.Interfaces;
using PetShop.Application.Features.Pet.Interfaces;

namespace PetShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PetsController : ControllerBase
{
	private readonly IPetService _petService;

	public PetsController(IPetService petService)
	{
		_petService = petService;
	}

	[HttpGet]
	public async Task<ActionResult<IEnumerable<PetDto>>> GetPets()
	{
		var pets = await _petService.GetAllPetsAsync();
		return Ok(pets);
	}

	[HttpGet("{id}")]
	public async Task<ActionResult<PetDto>> GetPet(int id)
	{
		var pet = await _petService.GetPetAsync(id);
		if (pet == null)
			return NotFound();

		return Ok(pet);
	}

	[HttpPost]
	public async Task<ActionResult<PetDto>> CreatePet(CreatePetDto createPetDto)
	{
		try
		{
			var pet = await _petService.CreatePetAsync(createPetDto);
			return CreatedAtAction(nameof(GetPet), new { id = pet.Id }, pet);
		}
		catch (Exception ex)
		{
			return BadRequest(ex.Message);
		}
	}

	[HttpPut("{id}")]
	public async Task<IActionResult> UpdatePet(int id, UpdatePetDto updatePetDto)
	{
		try
		{
			var pet = await _petService.UpdatePetAsync(id, updatePetDto);
			if (pet == null)
				return NotFound();

			return Ok(pet);
		}
		catch (Exception ex)
		{
			return BadRequest(ex.Message);
		}
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> DeletePet(int id)
	{
		var result = await _petService.DeletePetAsync(id);
		if (result == null)
			return NotFound();

		return NoContent();
	}
}