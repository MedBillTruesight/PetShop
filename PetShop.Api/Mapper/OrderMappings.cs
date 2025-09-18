using PetShop.Api.Models;

namespace PetShop.Api.Mapper
{
    public static class OrderMappings
    {
        public static Order ToModel(this Dtos.CreateOrderDto dto)
        {
            return new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = dto.CustomerId,
                PickupDate = DateTime.UtcNow,
                Status = Enums.OrderStatus.Open,
                Pets =  dto.Pets?.Select(p => p.ToModel()).ToList() ?? new List<Pet>()
            };
        }

        public static Dtos.OrderDto ToDto(this Order order)
        {
            return new Dtos.OrderDto
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                PickupDate = order.PickupDate,
                Status = order.Status.ToString(),
                ActualCost = order.ActualCost,
                EstimatedCost = order.Status == Enums.OrderStatus.Delivered ? 0 : order.Pets.Sum(p => p.Price),
                Pets = order.Pets.Select(p => p.ToDto()).ToList()
            };
        }
    }
}
