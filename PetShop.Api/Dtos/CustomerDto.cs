namespace PetShop.Api.Dtos
{
    public class CustomerDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public decimal EstimatedDue { get; set; }
        public decimal ActualDue { get; set; }
    }
}
