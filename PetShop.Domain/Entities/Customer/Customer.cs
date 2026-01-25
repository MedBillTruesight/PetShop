
using PetShop.Domain.Common;

namespace PetShop.Domain.Entities;
public class Customer : BaseEntity
{
   public string FirstName { get; set; } = ""; 
   public string LastName { get; set; } = ""; 
   public string Email { get; set; } = ""; 
   public string Phone { get; set; } = ""; 
   public string Address { get; set; } = ""; 

   public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}