using AutoMapper;
using PetShop.Application.DTOs;
using PetShop.Domain.Entities;

namespace PetShop.Application.Profiles;
public class OrderMappingProfile: Profile
{
    public OrderMappingProfile()
    {
        CreateMap<Order, CreateOrderDto>().ReverseMap();
        CreateMap<UpdateOrderDto, Order>().ReverseMap();
        CreateMap<OrderPet, CreateOrderPetDto>().ReverseMap();

        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.Pets, opt => opt.MapFrom(src => src.OrderPets.Select(op => op.Pet)))
            .ForMember(dest => dest.Cost, opt => opt.MapFrom(src => src.ActualCost))
            .ForMember(dest => dest.EstimatedCost, opt => opt.MapFrom(src => src.OrderPets.Sum(op => op.Pet.Price)));
    }
}