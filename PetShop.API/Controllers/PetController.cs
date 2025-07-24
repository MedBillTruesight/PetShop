using Microsoft.AspNetCore.Mvc;
using PetShop.API.DTOs;
using PetShop.API.Services.Interfaces;

namespace PetShop.API.Controllers;

[ApiController]
[Route("api/pets")]
public class PetsController : ControllerBase
{
    private readonly IPetService _petService;

    public PetsController(IPetService petService)
    {
        _petService = petService;
    }

    [HttpGet]
    public async Task<ActionResult<List<PetDto>>> GetAll()
    {
        var pets = await _petService.GetAllAsync();
        return Ok(pets);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PetDto>> GetById(Guid id)
    {
        var pet = await _petService.GetByIdAsync(id);
        return pet == null ? NotFound() : Ok(pet);
    }

    [HttpPost]
    public async Task<ActionResult<PetDto>> Create(CreatePetDto dto)
    {
        var created = await _petService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
}