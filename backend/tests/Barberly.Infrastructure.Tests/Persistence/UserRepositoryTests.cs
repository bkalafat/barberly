using System.Threading.Tasks;
using Barberly.Domain.Entities;
using Barberly.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Barberly.Infrastructure.Tests.Persistence
{
    public class UserRepositoryTests
    {
        [Fact]
        public async Task Add_And_GetByEmail_Works()
        {
            var options = new DbContextOptionsBuilder<BarberlyDbContext>()
                .UseInMemoryDatabase(databaseName: "UserRepoTestDb")
                .Options;
            using var db = new BarberlyDbContext(options);
            var repo = new UserRepository(db);
            var user = new User
            {
                Id = System.Guid.NewGuid(),
                Email = "repo@example.com",
                PasswordHash = "hash",
                FullName = "Repo User",
                Role = "customer",
                CreatedAt = System.DateTime.UtcNow
            };
            await repo.AddAsync(user);
            var fetched = await repo.GetByEmailAsync("repo@example.com");
            Assert.NotNull(fetched);
            Assert.Equal("repo@example.com", fetched!.Email);
        }
    }
}
