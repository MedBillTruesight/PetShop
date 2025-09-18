namespace PetShop.Api.Dtos
{
    public class PetDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Kind { get; set; }
        public string? Color { get; set; }
        public decimal Price { get; set; }
        public string? Breed { get;  set; }
    }
}
