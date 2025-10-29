namespace Barberly.Application.Interfaces;

/// <summary>
/// Generates access tokens for authenticated users.
/// </summary>
public interface ITokenService
{
    string GenerateToken(string email, string role, string userId);
}

