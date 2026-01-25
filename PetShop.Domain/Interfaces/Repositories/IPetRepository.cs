using PetShop.Domain.Entities;

namespace PetShop.Domain.Interfaces.Repositories;
public interface IPetRepository
{
    Task<Pet> CreatePetAsync(Pet pet);
    Task<Pet?> UpdatePetAsync(Pet pet);
    Task<Pet?> GetPetByIdAsync(Guid id);
    Task<Pet?> DeletePetByIdAsync(Guid id);
    Task<List<Pet>> GetAllPetsAsync();

}