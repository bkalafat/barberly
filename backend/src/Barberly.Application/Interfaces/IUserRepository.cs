using System.Threading.Tasks;
using Barberly.Domain.Entities;

namespace Barberly.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task AddAsync(User user);
    }
}
