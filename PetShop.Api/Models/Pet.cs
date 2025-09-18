namespace PetShop.Api.Models
{
    public class Pet
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid OrderId { get; set; }
        public Order? Order { get; set; }

        public string Name { get; set; } = null!;
        public string? Kind { get; set; }
        public string? Color { get; set; }
        public decimal Price { get; set; }
        public string? Breed { get; set; }
    }
}
