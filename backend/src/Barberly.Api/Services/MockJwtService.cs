using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Barberly.Application.Interfaces;

namespace Barberly.Api.Services;

public class MockJwtService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly string _secretKey = "this-is-a-very-long-secret-key-for-testing-purposes-only-do-not-use-in-production";

    public MockJwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(string email, string role, string userId)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        // Clear claim type mappings for this handler to preserve original types
        tokenHandler.InboundClaimTypeMap.Clear();
        tokenHandler.OutboundClaimTypeMap.Clear();

        var key = Encoding.ASCII.GetBytes(_secretKey);

        // Handle null/empty email values
        var safeEmail = email ?? "";
        var safeRole = (role ?? "").ToTitleCase() ?? "";
        var safeUserId = userId ?? "";

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, safeUserId),
            new Claim(ClaimTypes.Name, safeEmail),
            new Claim(ClaimTypes.Email, safeEmail),
            new Claim("extension_UserType", safeRole),
            new Claim("emails", safeEmail),
            new Claim(JwtRegisteredClaimNames.Sub, safeUserId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(24),
            Issuer = "https://barberly-dev.b2clogin.com/mock-tenant-id/v2.0/",
            Audience = "mock-api-client-id",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    // Helper method for testing - returns the validated token with proper claim types
    public ClaimsPrincipal ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secretKey);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = "https://barberly-dev.b2clogin.com/mock-tenant-id/v2.0/",
            ValidateAudience = true,
            ValidAudience = "mock-api-client-id",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
        return principal;
    }
}

public static class StringExtensions
{
    public static string? ToTitleCase(this string? input)
    {
        if (input == null)
            return null;

        if (string.IsNullOrEmpty(input))
            return input;

        return char.ToUpper(input[0]) + input.Substring(1).ToLower();
    }
}
