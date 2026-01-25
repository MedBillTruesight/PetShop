

using AutoMapper;
using PetShop.Application.DTOs;
using PetShop.Domain.Entities;

public class PetMappingProfile: Profile
{
    public PetMappingProfile()
    {
            CreateMap<Pet, PetDto>().ReverseMap();
            CreateMap<Pet, CreatePetDto>().ReverseMap();
            CreateMap<UpdatePetDto, Pet>().ReverseMap();
    }
}