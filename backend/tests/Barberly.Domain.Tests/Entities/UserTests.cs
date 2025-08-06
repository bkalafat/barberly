using System;
using Barberly.Domain.Entities;
using Xunit;

namespace Barberly.Domain.Tests.Entities
{
    public class UserTests
    {
        [Fact]
        public void User_Constructor_SetsProperties()
        {
            var id = Guid.NewGuid();
            var user = new User
            {
                Id = id,
                Email = "test@example.com",
                PasswordHash = "hash",
                FullName = "Test User",
                Role = "customer",
                CreatedAt = DateTime.UtcNow
            };
            Assert.Equal(id, user.Id);
            Assert.Equal("test@example.com", user.Email);
            Assert.Equal("hash", user.PasswordHash);
            Assert.Equal("Test User", user.FullName);
            Assert.Equal("customer", user.Role);
        }
    }
}
