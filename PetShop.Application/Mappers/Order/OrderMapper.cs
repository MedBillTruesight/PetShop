using PetShop.Application.DTOs;
using PetShop.Domain.Entities;
using AutoMapper;

namespace PetShop.Application.Mappers;

public class OrderMapper : IOrderMapper
{
    private readonly IMapper _mapper;

    public OrderMapper(IMapper mapper)
    {
        _mapper = mapper;
    }

    public Order ToDomain(CreateOrderDto dto)
        => _mapper.Map<Order>(dto);

    public Order ToDomain(UpdateOrderDto dto)
        => _mapper.Map<Order>(dto);

    public OrderPet ToDomain(CreateOrderPetDto dto)
    => _mapper.Map<OrderPet>(dto);

    public OrderDto ToDto(Order order)
     => _mapper.Map<OrderDto>(order);

}