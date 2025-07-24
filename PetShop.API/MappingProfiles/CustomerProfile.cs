using AutoMapper;
using PetShop.API.DTOs;
using PetShop.API.Models;

public class CustomerProfile : Profile
{
    public CustomerProfile()
    {
        CreateMap<Customer, CustomerDto>()
            .ForMember(dest => dest.FullName,
                opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}".Trim()));
    }
}