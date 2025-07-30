using AutoMapper;
using PetShop.Application.DTOs;
using PetShop.Domain.Entities;


namespace PetShop.Application.MappingProfiles;

public class OrderMappingProfile: Profile
{
    public OrderMappingProfile()
    {
        // CreateMap<Order, OrderDto>().ReverseMap();
        CreateMap<Order, CreateOrderDto>().ReverseMap();
        CreateMap<UpdateOrderDto, Order>().ReverseMap();
        CreateMap<OrderPet, CreateOrderPetDto>().ReverseMap();

        // handle mapping of pets from order.OrderPets.Pet
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.Pets, opt => opt.MapFrom(src => src.OrderPets.Select(op => op.Pet)))
            .ForMember(dest => dest.Cost, opt => opt.MapFrom(src => src.ActualCost))
            .ForMember(dest => dest.EstimatedCost, opt => opt.MapFrom(src => src.OrderPets.Sum(op => op.Pet.Price)));

        // CreateMap<OrderPetDto, CreateOrderPetDto>()
        //     .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.OrderId))
        //     .ForMember(dest => dest.PetId, opt => opt.MapFrom(src => src.PetId));
        // CreateMap<CreateOrderPetDto, OrderPetDto>()
        //     .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.OrderId))
        //     .ForMember(dest => dest.PetId, opt => opt.MapFrom(src => src.PetId));
    }
}