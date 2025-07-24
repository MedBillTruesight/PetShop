namespace PetShop.API.DTOs;

public class OrderDto
{
    public Guid Id { get; set; }
    public DateTime PickupDate { get; set; }
    public string Status { get; set; } = "Open";
    public List<PetDto> Pets { get; set; } = new();
    public decimal? EstimatedCost { get; set; }
    public decimal? ActualCost { get; set; }
    public CustomerDto Customer { get; set; } = new();
}

public class CreateOrderDto
{
    public Guid CustomerId { get; set; }
    public DateTime PickupDate { get; set; }
    public List<Guid> PetIds { get; set; } = new();
}

public class UpdateOrderDto
{
    public DateTime? PickupDate { get; set; }
    public string? Status { get; set; }
    public List<Guid>? PetIds { get; set; }
}