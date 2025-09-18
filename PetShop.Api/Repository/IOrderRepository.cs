using Microsoft.EntityFrameworkCore.Storage;
using PetShop.Api.Models;

namespace PetShop.Api.Repository
{
    public interface IOrderRepository
    {
        Task<Order> AddAsync(Order order);
        Task<Order> GetById(Guid id);
        Task<Order> Update(Order order);
        Task<bool> Delete(Guid id);
        Task<IEnumerable<Order>> GetByCustomerId(Guid customerId);

        Task<Pet> AddPetAsync(Pet pet);
        Task<Pet> GetPetById(Guid id);
        Task<bool> RemovePet(Guid id);
    }
}
