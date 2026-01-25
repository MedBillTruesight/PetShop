using AutoMapper;
using PetShop.Application.DTOs;
using PetShop.Application.Interfaces.Mappers;
using PetShop.Domain.Entities;

namespace PetShop.Application.Mappers;
public class PetMapper: IPetMapper
{
    private readonly IMapper _mapper;

    public PetMapper(IMapper mapper)
    {
        _mapper = mapper;
    }

    public Pet ToDomain(CreatePetDto dto) => _mapper.Map<Pet>(dto);

    public Pet ToDomain(UpdatePetDto dto) => _mapper.Map<Pet>(dto);

    public PetDto ToDto(Pet pet) => _mapper.Map<PetDto>(pet);
}