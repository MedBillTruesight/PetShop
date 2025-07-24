using Microsoft.EntityFrameworkCore;
using PetShop.API.Data;
using PetShop.API.DTOs;
using PetShop.API.Models;
using PetShop.API.Services.Interfaces;

namespace PetShop.API.Services;

public class PetService : IPetService
{
    private readonly PetShopContext _context;

    public PetService(PetShopContext context)
    {
        _context = context;
    }

    public async Task<List<PetDto>> GetAllAsync()
    {
        return await _context.Pets
            .Select(p => new PetDto
            {
                Id = p.Id,
                Name = p.Name,
                Kind = p.Kind,
                Color = p.Color,
                Price = p.Price
            }).ToListAsync();
    }

    public async Task<PetDto?> GetByIdAsync(Guid id)
    {
        var pet = await _context.Pets.FindAsync(id);
        return pet == null ? null : new PetDto
        {
            Id = pet.Id,
            Name = pet.Name,
            Kind = pet.Kind,
            Color = pet.Color,
            Price = pet.Price
        };
    }

    public async Task<PetDto> CreateAsync(CreatePetDto dto)
    {
        var pet = new Pet
        {
            Name = dto.Name,
            Kind = dto.Kind,
            Color = dto.Color,
            Price = dto.Price
        };

        _context.Pets.Add(pet);
        await _context.SaveChangesAsync();

        return new PetDto
        {
            Id = pet.Id,
            Name = pet.Name,
            Kind = pet.Kind,
            Color = pet.Color,
            Price = pet.Price
        };
    }
}