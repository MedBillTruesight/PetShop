using PetShop.Api.Enums;

namespace PetShop.Api.Models
{
    public class Order
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public DateTime PickupDate { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Open;

        public decimal? ActualCost { get; set; }
        public ICollection<Pet> Pets { get; set; } = new List<Pet>();
    }
}
