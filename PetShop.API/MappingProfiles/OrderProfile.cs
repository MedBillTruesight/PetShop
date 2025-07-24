using AutoMapper;
using PetShop.API.DTOs;
using PetShop.API.Models;

namespace PetShop.API.MappingProfiles;

public class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<Pet, PetDto>();

        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.EstimatedCost, opt => opt.MapFrom(src =>
                src.Status == OrderStatus.Delivered ? (decimal?)null : src.Pets.Sum(p => p.Price)))
            .ForMember(dest => dest.ActualCost, opt => opt.MapFrom(src =>
                src.Status == OrderStatus.Delivered ? src.ActualTotalCost : (decimal?)null))
            .ForMember(dest => dest.Customer, opt => opt.MapFrom(src => src.Customer));

        CreateMap<Customer, CustomerDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
            .ForMember(dest => dest.EstimatedPaymentDue, opt => opt.Ignore())
            .ForMember(dest => dest.ActualPaymentDue, opt => opt.Ignore());
    }
}