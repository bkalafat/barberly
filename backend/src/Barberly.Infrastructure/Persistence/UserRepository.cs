using Barberly.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Barberly.Infrastructure.Persistence
{
    public class UserRepository : Barberly.Application.Interfaces.IUserRepository
    {
        private readonly BarberlyDbContext _db;
        public UserRepository(BarberlyDbContext db)
        {
            _db = db;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task AddAsync(User user)
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }
    }
}
