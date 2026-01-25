using PetShop.Application.DTOs;
using PetShop.Domain.Entities;

namespace PetShop.Application.Interfaces.Mappers;
public interface IPetMapper
{
    Pet ToDomain(CreatePetDto dto);
    Pet ToDomain(UpdatePetDto dto);
    PetDto ToDto(Pet pet);
}