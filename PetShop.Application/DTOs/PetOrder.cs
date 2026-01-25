
namespace PetShop.Application.DTOs;
public class CreateOrderPetDto
{
    public Guid OrderId { get; set; }
    public Guid PetId { get; set; }
}

public class RemoveOrderPetDto
{
    public Guid OrderId { get; set; }
    public Guid PetId { get; set; }
}