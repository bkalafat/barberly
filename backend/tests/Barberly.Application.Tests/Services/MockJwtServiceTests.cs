using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Barberly.Api.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace Barberly.Application.Tests.Services;

/// <summary>
/// Tests for MockJwtService functionality
/// </summary>
public class MockJwtServiceTests
{
    private readonly MockJwtService _jwtService;
    private readonly IConfiguration _configuration;

    public MockJwtServiceTests()
    {
        var configurationBuilder = new ConfigurationBuilder();
        _configuration = configurationBuilder.Build();
        _jwtService = new MockJwtService(_configuration);
    }

    [Fact]
    public void GenerateToken_ValidInput_ShouldReturnValidJwtToken()
    {
        // Arrange
        var email = "test@example.com";
        var role = "customer";
        var userId = Guid.NewGuid().ToString();

        // Act
        var token = _jwtService.GenerateToken(email, role, userId);

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Split('.').Should().HaveCount(3, "JWT tokens should have 3 parts separated by dots");
    }

    [Fact]
    public void GenerateToken_ValidInput_ShouldContainCorrectClaims()
    {
        // Arrange
        var email = "test@example.com";
        var role = "customer";
        var userId = Guid.NewGuid().ToString();

        // Act
        var token = _jwtService.GenerateToken(email, role, userId);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);

        // Check claims
        jsonToken.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == userId);
        jsonToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == email);
        jsonToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == email);
        jsonToken.Claims.Should().Contain(c => c.Type == "extension_UserType" && c.Value == "Customer");
        jsonToken.Claims.Should().Contain(c => c.Type == "emails" && c.Value == email);
        jsonToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == userId);
        jsonToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Jti);
    }

    [Fact]
    public void GenerateToken_ValidInput_ShouldHaveCorrectIssuerAndAudience()
    {
        // Arrange
        var email = "test@example.com";
        var role = "barber";
        var userId = Guid.NewGuid().ToString();

        // Act
        var token = _jwtService.GenerateToken(email, role, userId);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);

        jsonToken.Issuer.Should().Be("https://barberly-dev.b2clogin.com/mock-tenant-id/v2.0/");
        jsonToken.Audiences.Should().Contain("mock-api-client-id");
    }

    [Fact]
    public void GenerateToken_ValidInput_ShouldHaveValidExpiration()
    {
        // Arrange
        var email = "test@example.com";
        var role = "admin";
        var userId = Guid.NewGuid().ToString();

        // Act
        var token = _jwtService.GenerateToken(email, role, userId);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);

        jsonToken.ValidTo.Should().BeAfter(DateTime.UtcNow);
        jsonToken.ValidTo.Should().BeBefore(DateTime.UtcNow.AddHours(25));
    }

    [Theory]
    [InlineData("customer", "Customer")]
    [InlineData("barber", "Barber")]
    [InlineData("shop_owner", "Shop_owner")]
    [InlineData("admin", "Admin")]
    [InlineData("CUSTOMER", "Customer")]
    [InlineData("BARBER", "Barber")]
    public void GenerateToken_DifferentRoles_ShouldFormatCorrectly(string inputRole, string expectedRole)
    {
        // Arrange
        var email = "test@example.com";
        var userId = Guid.NewGuid().ToString();

        // Act
        var token = _jwtService.GenerateToken(email, inputRole, userId);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);

        var userTypeClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "extension_UserType");
        userTypeClaim.Should().NotBeNull();
        userTypeClaim!.Value.Should().Be(expectedRole);
    }

    [Fact]
    public void GenerateToken_MultipleCallsWithSameInput_ShouldReturnDifferentTokens()
    {
        // Arrange
        var email = "test@example.com";
        var role = "customer";
        var userId = Guid.NewGuid().ToString();

        // Act
        var token1 = _jwtService.GenerateToken(email, role, userId);
        var token2 = _jwtService.GenerateToken(email, role, userId);

        // Assert
        token1.Should().NotBe(token2, "Each token should have a unique JTI claim");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void GenerateToken_InvalidEmail_ShouldStillGenerateToken(string email)
    {
        // Arrange
        var role = "customer";
        var userId = Guid.NewGuid().ToString();

        // Act
        var token = _jwtService.GenerateToken(email, role, userId);

        // Assert
        token.Should().NotBeNullOrEmpty();
        
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);
        jsonToken.Should().NotBeNull();
    }
}

/// <summary>
/// Tests for StringExtensions utility methods
/// </summary>
public class StringExtensionsTests
{
    [Theory]
    [InlineData("customer", "Customer")]
    [InlineData("barber", "Barber")]
    [InlineData("shop_owner", "Shop_owner")]
    [InlineData("admin", "Admin")]
    [InlineData("CUSTOMER", "Customer")]
    [InlineData("BARBER", "Barber")]
    [InlineData("test", "Test")]
    [InlineData("", "")]
    public void ToTitleCase_VariousInputs_ShouldReturnCorrectFormat(string input, string expected)
    {
        // Act
        var result = input.ToTitleCase();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ToTitleCase_NullInput_ShouldReturnNull()
    {
        // Arrange
        string input = null!;

        // Act
        var result = input.ToTitleCase();

        // Assert
        result.Should().BeNull();
    }
}
