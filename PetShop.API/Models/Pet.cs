using System;
using System.ComponentModel.DataAnnotations;

namespace PetShop.API.Models
{
    public class Pet
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(50)]
        public string Name { get; set; }

        [Required]
        public string Kind { get; set; }

        public string? Breed { get; set; }

        public string? Color { get; set; }

        public string? Gender { get; set; }

        public int? Age { get; set; }

        [Required, Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        public Guid OrderId { get; set; }

        public Order Order { get; set; }
    }
}