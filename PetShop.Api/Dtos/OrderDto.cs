namespace PetShop.Api.Dtos
{
    public class OrderDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime PickupDate { get; set; }
        public string Status { get; set; } = null!;
        public decimal? ActualCost { get; set; }
        public decimal? EstimatedCost { get; set; }
        public List<PetDto> Pets { get; set; } = new();
    }
}
