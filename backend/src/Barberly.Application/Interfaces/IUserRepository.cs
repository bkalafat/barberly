using System.Threading.Tasks;
using Barberly.Domain.Entities;

namespace Barberly.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task AddAsync(User user);
    }
}
