using Microsoft.EntityFrameworkCore.Storage;
using PetShop.Api.Models;

namespace PetShop.Api.Repository
{
    public interface ICustomerRepository
    {
        Task<Customer> AddAsync(Customer customer);
        Task<Customer> GetById(Guid id);
        Task<Customer> Update(Customer customer);
        Task<bool> Delete(Guid id);
        Task<IEnumerable<Customer>> GetAll();
        Task<bool> ExistsAsync(Guid id);
    }
}
