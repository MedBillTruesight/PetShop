using AutoMapper;
using PetShop.Application.DTOs;
using PetShop.Domain.Entities;

namespace PetShop.Application.Profiles;
public class CustomerMappingProfile: Profile
{
    public CustomerMappingProfile()
    {
            CreateMap<Customer, CustomerDto>().ReverseMap();
            CreateMap<Customer, CreateCustomerDto>().ReverseMap();
            CreateMap<UpdateCustomerDto, Customer>().ReverseMap();
    }
}