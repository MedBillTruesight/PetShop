using PetShop.Application.DTOs;
using PetShop.Domain.Entities;

namespace PetShop.Application.Mappers;

public interface IOrderMapper
{
    Order ToDomain(CreateOrderDto dto);
    Order ToDomain(UpdateOrderDto dto);
    OrderPet ToDomain(CreateOrderPetDto dto);
    OrderDto ToDto(Order order);
}