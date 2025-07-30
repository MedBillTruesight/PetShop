using PetShop.Domain.Enums;

namespace PetShop.Application.DTOs;

public class OrderDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime PickupDate { get; set; }
    public OrderStatus Status { get; set; }
    public decimal Cost { get; set; }
    public decimal EstimatedCost { get; set; }
    public List<PetDto> Pets { get; set; } = new();
}

public class CreateOrderDto
{
    public int CustomerId { get; set; }
    public DateTime PickupDate { get; set; }
}

public class UpdateOrderDto
{
    public DateTime? PickupDate { get; set; }
    public OrderStatus? Status { get; set; }
}
