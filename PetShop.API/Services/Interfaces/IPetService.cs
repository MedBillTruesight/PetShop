using PetShop.API.DTOs;

namespace PetShop.API.Services.Interfaces;

public interface IPetService
{
    Task<List<PetDto>> GetAllAsync();
    Task<PetDto?> GetByIdAsync(Guid id);
    Task<PetDto> CreateAsync(CreatePetDto dto);
}