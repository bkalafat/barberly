using Barberly.Api.Services;
using Xunit;

namespace Barberly.Api.Tests.Services
{
    public class PasswordHasherTests
    {
        [Fact]
        public void HashPassword_And_VerifyPassword_Works()
        {
            var hasher = new PasswordHasher();
            var password = "MySecret123!";
            var hash = hasher.HashPassword(password);
            Assert.True(hasher.VerifyPassword(password, hash));
            Assert.False(hasher.VerifyPassword("WrongPassword", hash));
        }
    }
}
