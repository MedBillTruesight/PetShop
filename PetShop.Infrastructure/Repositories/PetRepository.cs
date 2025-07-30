using Microsoft.EntityFrameworkCore;
using PetShop.Domain.Entities;
using PetShop.Domain.Interfaces;
using PetShop.Infrastructure.Persistence;

namespace PetShop.Infrastructure.Repositories;

public class PetRepository: IPetRepository
{
    private readonly PetShopDbContext _context;

    public PetRepository(PetShopDbContext context)
    {
        _context = context;
    }

    public async Task<Pet> CreatePetAsync(Pet pet)
    {
        await _context.Pets.AddAsync(pet);
        await _context.SaveChangesAsync();
        return pet;
    }

    public async Task<Pet?> UpdatePetAsync(Pet pet)
    {
        var existingPet = await _context.Pets.FirstOrDefaultAsync(c => c.Id == pet.Id);

        if (existingPet != null)
        {
            existingPet.Name = pet.Name;
            existingPet.Price = pet.Price;
            existingPet.Kind = pet.Kind;
            existingPet.Color = pet.Color;
            existingPet.Breed = pet.Breed;
            existingPet.AgeInMonths = pet.AgeInMonths;
            existingPet.Description = pet.Description;

        }
        await _context.SaveChangesAsync();
        return existingPet;
    }

    public async Task<Pet?> GetPetByIdAsync(int id)
    {
        return  await _context.Pets
            .Include(c => c.OrderPets)
            .ThenInclude(o => o.Order)
            .FirstOrDefaultAsync(c=>c.Id == id);
    }

    public async Task<Pet?> DeletePetByIdAsync(int id)
    {
        var petToDelete = await _context.Pets.FirstOrDefaultAsync(c => c.Id == id);

        if (petToDelete == null)
        {
            return null;
        }

        _context.Pets.Remove(petToDelete);
        await _context.SaveChangesAsync();
        return petToDelete;
    }

    public async Task<List<Pet>> GetAllPetsAsync()
    {
        return await _context.Pets.AsNoTracking().ToListAsync();
    }
}