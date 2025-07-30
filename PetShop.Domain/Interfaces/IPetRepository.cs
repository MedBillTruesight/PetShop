using PetShop.Domain.Entities;

namespace PetShop.Domain.Interfaces;

public interface IPetRepository
{
    Task<Pet> CreatePetAsync(Pet pet);
    Task<Pet?> UpdatePetAsync(Pet pet);
    Task<Pet?> GetPetByIdAsync(int id);
    Task<Pet?> DeletePetByIdAsync(int id);
    Task<List<Pet>> GetAllPetsAsync();

}