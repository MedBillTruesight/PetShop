using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PetShop.API.Models
{
    public enum OrderStatus
    {
        Open,
        Processing,
        Delivered
    }

    public class Order
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid CustomerId { get; set; }

        public Customer Customer { get; set; }

        [Required]
        public DateTime PickupDate { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Open;

        public ICollection<Pet> Pets { get; set; } = new List<Pet>();

        public decimal? ActualTotalCost { get; set; }

        public bool IsPaid { get; set; } = false;

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}